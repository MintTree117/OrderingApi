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
    IConfiguration config, IHttpService http, IOrderingRepository repo, IEmailSender emailSender, UserManager<UserAccount> userManager, ILogger<OrderingSystem> logger )
{
    readonly IHttpService _http = http;
    readonly IOrderingRepository _repo = repo;
    readonly IEmailSender _emailSender = emailSender;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly string _checkCatalogUrl = config.GetValue<string>( "Ordering:CheckCatalogUrl" ) ?? throw new Exception( "Failed to get CheckCatalogUrl from Configuration." );

    internal async Task<Reply<bool>> PlaceOrder( string? userId, OrderPlacementRequest request )
    {
        // TODO: Implement guest ordering
        if (string.IsNullOrWhiteSpace( userId ))
            return IReply.UserNotFound();
        
        UserAccount? user = await _userManager.FindByIdAsync( userId );
        if (user is null)
            return IReply.UserNotFound();
        
        CatalogOrderDto catalogDto = new( 
            request.ShippingAddress.PosX, request.ShippingAddress.PosY, request.Items );
        var catalogReply = await _http.TryPostObjRequest<List<OrderCatalogItem>>( _checkCatalogUrl, catalogDto );
        if (!catalogReply)
            return IReply.ServerError( catalogReply.GetMessage() );
        
        Order order = GenerateOrderModel( userId, request, catalogReply.Data );

        /*var warehouseReply = await SendOrdersToWarehouses( userId, order, orderLines );
        if (!warehouseReply)
            return IReply.ServerError( warehouseReply.GetMessage() );*/
        
        var insertReply = await _repo.InsertOrder( order ); //await InsertOrderModels( order, orderGroups, orderLines );
        if (!insertReply)
            return IReply.ServerError();
        
        string email = OrderingEmailUtility.GenerateOrderPlacedEmail( order );
        _emailSender.SendHtmlEmail( user.Email!, "Order Placed", email );
        return insertReply;
    }
    static Order GenerateOrderModel( string? userId,  OrderPlacementRequest request, List<OrderCatalogItem> catalogItems )
    {
        Order order = Order.New( userId, request.GetContact(), request.BillingAddress, request.ShippingAddress );
        HashSet<OrderGroup> orderGroups = [];
        Dictionary<OrderGroup, HashSet<OrderLine>> orderLines = [];

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

        foreach ( var kvp in orderLines )
            kvp.Key.OrderLines = kvp.Value.ToList();
        order.OrderGroups = orderGroups.ToList();

        return order;
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
            
            // TODO: IMPLEMENT
            var warehouseReply = true; // await _http.TryPostObjRequest<bool>( "", warehouseOrder );
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
    
    internal async Task<Reply<bool>> UpdateOrder( OrderUpdateRequest update )
    {
        var orderReply = await _repo.GetOrderById( update.OrderId );
        if (!orderReply)
            return IReply.NotFound( "Order not found." );

        var orderGroupsReply = await _repo.GetOrderGroupsForOrder( update.OrderId );
        if (!orderGroupsReply)
            return IReply.NotFound( "OrderGroup not found." );

        string email = orderReply.Data.Contact.Email;
        var groupUpdate = await HandleGroupUpdate( email, orderGroupsReply.Enumerable, update.OrderGroupId, update.OrderState );
        if (!groupUpdate)
            return IReply.Fail( groupUpdate.GetMessage() );

        var orderUpdate = await HandleOrderUpdate( email, orderReply.Data, orderGroupsReply.Enumerable, update.OrderState );
        if (!orderUpdate)
            return IReply.Fail( orderUpdate.GetMessage() );
        
        var dbReply = await _repo.SaveAsync();
        return dbReply
            ? IReply.Success()
            : IReply.ServerError( dbReply.GetMessage() );
    }
    async Task<Reply<bool>> HandleGroupUpdate( string emailAddress, IEnumerable<OrderGroup> groups, Guid groupId, OrderState state )
    {
        OrderGroup? group = groups.FirstOrDefault( g => g.Id == groupId );
        if (group is null)
            return IReply.NotFound( "OrderGroup not found." );

        string email = OrderingEmailUtility.GenerateOrderGroupUpdateEmail( group, state );
        _emailSender.SendHtmlEmail( emailAddress, "Order Placed", email );
        
        group.Update( state );
        return await _repo.SaveAsync();
    }
    async Task<Reply<bool>> HandleOrderUpdate( string emailAddress, Order order, IEnumerable<OrderGroup> groups, OrderState state )
    {
        bool same = groups.All( g => g.State == state );
        if (!same)
            return IReply.Success();
        
        string email = OrderingEmailUtility.GenerateOrderUpdateEmail( order, state );
        _emailSender.SendHtmlEmail( emailAddress, "Order Updated", email );
            
        order.State = state;
        return await _repo.SaveAsync();
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
        string email = OrderingEmailUtility.GenerateOrderCancelledEmail( orderReply.Data, request.Message );
        _emailSender.SendHtmlEmail( orderReply.Data.Contact.Email, "Order Cancelled", email );
        return problemReply;
    }

    readonly record struct CatalogOrderDto(
        int PosX,
        int PosY,
        List<CartItemDto> Items );
}