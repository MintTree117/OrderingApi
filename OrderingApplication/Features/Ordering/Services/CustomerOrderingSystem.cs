using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Cart;
using OrderingApplication.Features.Ordering.Dtos;
using OrderingDomain.Orders;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingInfrastructure.Email;
using OrderingInfrastructure.Features.Ordering.Repositories;
using OrderingInfrastructure.Http;

namespace OrderingApplication.Features.Ordering.Services;

internal sealed class CustomerOrderingSystem( 
    IConfiguration config, 
    IHttpService http, 
    ICustomerOrderingRepository customerRepository,
    WarehouseOrderingSystem warehouseSystem, 
    IEmailSender emailSender, 
    UserManager<UserAccount> userManager, 
    ILogger<CustomerOrderingSystem> logger )
{
    readonly string _checkCatalogUrl = config.GetValue<string>( "Ordering:CheckCatalogUrl" ) ?? throw new Exception( "Failed to get CheckCatalogUrl from Configuration." );
    readonly IHttpService _http = http;
    readonly ICustomerOrderingRepository _customerRepository = customerRepository;
    readonly WarehouseOrderingSystem _warehouseSystem = warehouseSystem;
    readonly IEmailSender _emailSender = emailSender;
    readonly UserManager<UserAccount> _userManager = userManager;
    
    internal async Task<Reply<bool>> PlaceOrderForGuest( OrderPlacementRequest request )
    {
        if (string.IsNullOrWhiteSpace( request.EncodedPaymentInfo ))
            return IReply.BadRequest( "Invalid payment information." );

        var reply = await PlaceOrder( null, request );
        return reply;
    }
    internal async Task<Reply<bool>> PlaceOrderForUser( string? userId, OrderPlacementRequest request )
    {
        UserAccount? user = await _userManager.FindByIdAsync( userId ?? string.Empty );
        if (user is null)
            return IReply.UserNotFound();

        var reply = await PlaceOrder( user.Id, request );
        return reply;
    }
    async Task<Reply<bool>> PlaceOrder( string? userId, OrderPlacementRequest request )
    {
        CatalogOrderDto catalogDto = new(
            request.ShippingAddress.PosX, request.ShippingAddress.PosY, request.Items );
        
        var catalogReply = await _http.TryPostObjRequest<List<OrderCatalogItem>>( _checkCatalogUrl, catalogDto );
        if (!catalogReply)
            return IReply.ServerError( catalogReply.GetMessage() );

        Order order = GenerateOrderModel( userId, request, catalogReply.Data );

        var dbReply = await InsertNewOrder( order );
        if (!dbReply)
            return dbReply;
        
        string body = OrderingEmailUtility.GenerateOrderPlacedEmail( order );
        _emailSender.SendHtmlEmail( "martygrof3708@gmail.com", "Order Placed", body );
        return dbReply;
    }
    static Order GenerateOrderModel( string? userId, OrderPlacementRequest request, List<OrderCatalogItem> catalogItems )
    {
        Order order = Order.New( userId, request.Contact, request.BillingAddress, request.ShippingAddress );
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

            OrderLine line = new() {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                OrderGroupId = group.Id,
                WarehouseId = item.WarehouseId,
                UnitId = item.ProductId,
                UnitName = item.UnitName,
                UnitPrice = item.UnitPrice,
                UnitDiscount = item.UnitDiscount,
                ShippingCost = item.ShippingCost,
                Quantity = item.Quantity
            };

            if (orderGroups.Add( group ))
                orderLines.Add( group, [] );
            orderLines[group].Add( line );
        }

        foreach ( var kvp in orderLines )
            kvp.Key.OrderLines = kvp.Value.ToList();
        order.OrderGroups = orderGroups.ToList();
        
        // TODO: Find a better place/way for this
        order.CalculatePricing();
        return order;
    }
    async Task<Reply<bool>> InsertNewOrder( Order order )
    {
        var warehouseTask = _warehouseSystem.InsertNewWarehouseOrderGroups( order );
        var customerTask = _customerRepository.InsertOrder( order );
        List<Task> dbTasks = [warehouseTask, customerTask];
        await Task.WhenAll( dbTasks );

        if (!warehouseTask.Result)
            return IReply.ServerError( warehouseTask.Result.GetMessage() );
        if (!customerTask.Result)
            return IReply.ServerError( customerTask.Result.GetMessage() );

        return IReply.Success();
    }
    
    internal async Task<Reply<bool>> CancelOrder( OrderCancelRequest request  )
    {
        var orderReply = await _customerRepository.GetOrderById( request.OrderId );
        if (!orderReply)
            return IReply.NotFound( "Order not found." );

        var deleteReply = await _customerRepository.DeleteOrderData( request.OrderId );
        if (!deleteReply)
            return IReply.ServerError( deleteReply.GetMessage() );
        
        OrderProblem problem = new() {
            OrderId = request.OrderId,
            OrderGroupId = request.OrderGroupId,
            Message = request.Message
        };
        var problemReply = await _customerRepository.InsertOrderProblem( problem );
        string email = OrderingEmailUtility.GenerateOrderCancelledEmail( orderReply.Data, request.Message );
        _emailSender.SendHtmlEmail( orderReply.Data.CustomerEmail, "Order Cancelled", email );
        return problemReply;
    }

    readonly record struct CatalogOrderDto(
        // ReSharper disable once NotAccessedPositionalProperty.Local
        int PosX,
        // ReSharper disable once NotAccessedPositionalProperty.Local
        int PosY,
        // ReSharper disable once NotAccessedPositionalProperty.Local
        List<CartItemDto> Items );
}