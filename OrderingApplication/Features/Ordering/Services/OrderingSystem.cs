using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Cart;
using OrderingApplication.Features.Ordering.Dtos;
using OrderingDomain.Orders.Base;
using OrderingDomain.Orders.Meta;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingInfrastructure.Email;
using OrderingInfrastructure.Features.Ordering.Repositories;
using OrderingInfrastructure.Http;

namespace OrderingApplication.Features.Ordering.Services;

internal sealed class OrderingSystem( 
    IConfiguration config, IHttpService http, IOrderingRepository repo, IEmailSender emailSender, UserManager<UserAccount> userManager )
{
    readonly IHttpService _http = http;
    readonly IOrderingRepository _repo = repo;
    readonly IEmailSender _emailSender = emailSender;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly string _checkCatalogUrl = config.GetValue<string>( "Ordering:CheckCatalogUrl" ) ?? string.Empty;

    internal async Task<Reply<bool>> PlaceOrder( string? userId, OrderPlacementRequest request )
    {
        // TODO: Implement guest ordering
        if (string.IsNullOrWhiteSpace( userId ))
            return IReply.UserNotFound();
        
        UserAccount? user = await _userManager.FindByIdAsync( userId );
        if (user is null)
            return IReply.UserNotFound();

        var catalogReply = await _http.TryPostObjRequest<List<OrderCatalogItem>>( _checkCatalogUrl, request.Items );
        if (!catalogReply)
            return IReply.ServerError( catalogReply.GetMessage() );
        
        GenerateOrderModels( userId, request, catalogReply.Data, out var order, out var orderGroups, out var orderLines );

        var warehouseReply = await SendOrdersToWarehouses( userId, order, orderLines );
        if (!warehouseReply)
            return IReply.ServerError( warehouseReply.GetMessage() );

        var insertReply = await InsertOrderModels( order, orderGroups, orderLines );

        if (!insertReply)
            return IReply.ServerError();
        
        SendOrderPlacedEmail();
        return insertReply;
    }
    static void GenerateOrderModels( string? userId, OrderPlacementRequest request, List<OrderCatalogItem> catalogItems, out Order order, out HashSet<OrderGroup> orderGroups, out Dictionary<OrderGroup, HashSet<OrderLine>> orderLines )
    {
        order = Order.New( userId, request.GetContact(), request.BillingAddress, request.ShippingAddress );
        orderGroups = [];
        orderLines = [];

        foreach ( OrderCatalogItem item in catalogItems )
        {
            OrderGroup group = orderGroups.FirstOrDefault(
                    g => g.WarehouseId == item.WarehouseId ) ??
                new OrderGroup() {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    WarehouseId = item.WarehouseId,
                    LastUpdated = DateTime.Now,
                };

            OrderLine line = new OrderLine() {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                OrderGroupId = group.Id,
                WarehouseId = item.WarehouseId,
                UnitId = item.ProductId,
                UnitPrice = item.UnitPrice,
                Discount = item.Discount,
                Tax = item.Tax,
                Quantity = item.Quantity
            };

            if (orderGroups.Add( group ))
                orderLines.Add( group, [] );
            orderLines[group].Add( line );
        }
    }
    async Task<Reply<bool>> SendOrdersToWarehouses( string? userId, Order order, Dictionary<OrderGroup, HashSet<OrderLine>> orderLines )
    {
        bool anyWarehousesFailed = false;
        
        foreach ( KeyValuePair<OrderGroup, HashSet<OrderLine>> kvp in orderLines )
        {
            List<CartItemDto> items = [];
            foreach ( var v in kvp.Value )
                items.Add( new CartItemDto( v.UnitId, v.Quantity ) );
            WarehouseOrder warehouseOrder = new(
                order.Id,
                kvp.Key.Id,
                userId,
                DateTime.Now,
                items );

            var warehouseReply = await _http.TryPostObjRequest<bool>( "", warehouseOrder );
            if (warehouseReply)
                continue;

            anyWarehousesFailed = true;
            break;
        }

        return !anyWarehousesFailed 
            ? IReply.Success() 
            : IReply.ServerError( "Your order did not go through." );
    }
    async Task<Reply<bool>> InsertOrderModels( Order order, HashSet<OrderGroup> orderGroups, Dictionary<OrderGroup, HashSet<OrderLine>> orderLines )
    {
        IReply orderReply = await _repo.InsertOrder( order );
        IReply groupsReply = await _repo.InsertOrderGroups( orderGroups );
        List<OrderLine> lines = [];
        foreach ( HashSet<OrderLine> l in orderLines.Values )
            lines.AddRange( l.ToList() );
        IReply linesReply = await _repo.InsertOrderLines( lines );

        if (orderReply.CheckSuccess() && groupsReply.CheckSuccess() && linesReply.CheckSuccess())
            return IReply.Success();

        OrderCancelRequest cancel = new( order.Id, null, $"{orderReply.GetMessage()} {groupsReply.GetMessage()} {linesReply.GetMessage()}" );
        await CancelOrder( cancel );
        return IReply.ServerError( "Your order did not go through." );
    }
    void SendOrderPlacedEmail()
    {
        
    }
    
    internal async Task<Reply<bool>> UpdateOrder( OrderUpdateRequest update )
    {
        var orderReply = await _repo.GetOrderById( update.OrderId );
        if (!orderReply)
            return IReply.NotFound( "Order not found." );

        var orderGroupsReply = await _repo.GetOrderGroupsForOrder( update.OrderId );
        if (!orderGroupsReply)
            return IReply.NotFound( "OrderGroup not found." );

        var groupUpdate = await HandleGroupUpdate( orderGroupsReply.Enumerable, update.OrderGroupId, update.OrderState );
        if (!groupUpdate)
            return IReply.Fail( groupUpdate.GetMessage() );

        var orderUpdate = await HandleOrderUpdate( orderReply.Data, orderGroupsReply.Enumerable, update.OrderState );
        if (!orderUpdate)
            return IReply.Fail( orderUpdate.GetMessage() );
        
        var dbReply = await _repo.SaveAsync();
        return dbReply
            ? IReply.Success()
            : IReply.ServerError( dbReply.GetMessage() );
    }
    async Task<Reply<bool>> HandleGroupUpdate( IEnumerable<OrderGroup> groups, Guid groupId, OrderState state )
    {
        OrderGroup? group = groups.FirstOrDefault( g => g.Id == groupId );
        if (group is null)
            return IReply.NotFound( "OrderGroup not found." );
        group.Update( state );
        SendOrderUpdatedEmail();
        return await _repo.SaveAsync();
    }
    async Task<Reply<bool>> HandleOrderUpdate( Order order, IEnumerable<OrderGroup> groups, OrderState state )
    {
        bool same = groups.All( g => g.State == state );
        if (!same)
            return IReply.Success();
        order.State = state;
        SendOrderUpdatedEmail();
        return await _repo.SaveAsync();
    }
    void SendOrderUpdatedEmail()
    {
        
    }
    
    internal async Task<Reply<bool>> CancelOrder( OrderCancelRequest request  )
    {
        var orderReply = await _repo.GetOrderById( request.OrderId );
        if (!orderReply)
            return IReply.NotFound( "Order not found." );

        var deleteReply = await _repo.DeleteOrderData( request.OrderId );
        if (!deleteReply)
            return IReply.ServerError( deleteReply.GetMessage() );
        
        OrderProblem problem = new() {
            OrderId = request.OrderId,
            OrderGroupId = request.OrderGroupId,
            Message = request.Message
        };
        var problemReply = await _repo.InsertOrderProblem( problem );
        SendOrderCancelledEmail();
        return problemReply;
    }
    void SendOrderCancelledEmail()
    {
        
    }
}