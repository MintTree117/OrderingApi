using OrderingApplication.Features.Ordering.Dtos;
using OrderingApplication.Features.Ordering.Services;
using OrderingDomain.Orders;
using OrderingDomain.ReplyTypes;
using OrderingDomain.ValueTypes;
using OrderingInfrastructure.Email;
using OrderingInfrastructure.Features.Ordering.Repositories;

namespace OrderingApplication.Features.Ordering.Systems;

internal sealed class OrderPlacingSystem( IOrderingRepository repo, IOrderingUtilityRepository utilityRepo, OrderLocationService locationService, IEmailSender emailSender )
{
    readonly IOrderingRepository _repo = repo;
    readonly IOrderingUtilityRepository _utilityRepo = utilityRepo;
    readonly OrderLocationService _locationService = locationService;
    readonly IEmailSender _emailSender = emailSender;

    // Public Interface (Start)
    internal async Task<Reply<OrderPlaceResponse>> PlaceOrder( OrderPlaceRequest dto )
    {
        ReplyLine<ItemOrderGroup> groups = new ReplyLine<ItemOrderGroup>();
        bool objIdOrder =
            (await MakeOrder( dto ))
            .OutSuccess( out Reply<Order> order ) &&
            (await MakeOrderItems( order.Data, dto ))
            .OutSuccess( out Replies<OrderItem> items ) &&
            (await MakeOrderLocations( order.Data, items.Enumerable ))
            .AsyncOut( out groups );

        return objIdOrder
            ? await HandleObjidOrder( order.Data, groups.ToObjects() )
            : await HandleConflictedOrder( order.Data, groups );
    }

    // Cancel Order
    async Task<Reply<OrderPlaceResponse>> CancelOrderRequest( Order order, IEnumerable<OrderLine>? lines, IReply originalError )
    {
        if ((await CancelOrderLines( lines )).OutSuccess( out Reply<bool> cancelLines ))
            return FailCancelOrderLines( originalError, cancelLines );

        return (await _repo.DeleteOrderData( order.Id ))
            .OutSuccess( out Reply<bool> deleteResult )
                ? CancelOrderSuccess( originalError )
                : FailDeleteOrderData( originalError, deleteResult );
    }
    async Task<Reply<bool>> CancelOrderLines( IEnumerable<OrderLine>? lines )
    {
        if (lines is null)
            return NoLinesToCancel();

        foreach ( OrderLine l in lines )
            await CancelOrderLine( l );

        return OrderLinesCancelled();
    }
    async Task CancelOrderLine( OrderLine line )
    {
        if ((await _locationService.ConfirmCancelOrderLine( line )).OutSuccess( out Reply<bool> f ))
            await _utilityRepo.InsertPendingCancelLine( line );
    }

    // Make Order
    async Task<Reply<Order>> MakeOrder( OrderPlaceRequest dto ) =>
        (await MakeOrder( dto.Order, dto.UserId )).OutSuccess( out Reply<Order> orderResult )
            ? await FailMakeOrder( orderResult )
            : orderResult;
    async Task<Replies<OrderItem>> MakeOrderItems( Order o, OrderPlaceRequest dto ) =>
        (await MakeOrderItems( o, dto.Items )).OutFailure( out Replies<OrderItem> itemsResult )
            ? await FailMakeOrderItems( o, itemsResult )
            : itemsResult;
    async Task<ReplyLine<ItemOrderGroup>> MakeOrderLocations( Order o, IEnumerable<OrderItem> items )
    {
        ReplyLine<ItemOrderGroup> multi = new();
        foreach ( OrderItem item in items )
            multi.Options.Add( (await FindNearestLocation( o.ShippingWorldGridPos, item.ProductId, item.Quantity ))
                .OutSuccess( out Reply<OrderLocation> opt )
                    ? MadeOrderLocation( item, opt.Data )
                    : FailedMakeOrderLocation() );
        return multi;
    }
    async Task<Reply<Order>> MakeOrder( OrderDto dto, Guid customerId )
    {
        Order order = OrderDto.Model( dto, customerId );
        return (await _repo.InsertOrder( order ))
            .OutSuccess( out Reply<bool> opt )
                ? Reply<Order>.Success( order )
                : Reply<Order>.Failure( opt );
    }
    async Task<Replies<OrderItem>> MakeOrderItems( Order order, IReadOnlyCollection<OrderItemDto> dtos )
    {
        List<OrderItem> items = OrderItemDto.Models( dtos );
        foreach ( OrderItem i in items )
            i.OrderId = order.Id;
        return (await _repo.InsertOrderItems( items ))
            .OutSuccess( out Reply<bool> opt )
                ? Replies<OrderItem>.Success( items )
                : Replies<OrderItem>.Fail( opt );
    }
    async Task<Reply<Order>> FailMakeOrder( Reply<Order> orderResult ) =>
        Reply<Order>.Failure( await CancelOrderRequest( orderResult.Data, null, orderResult ) );
    async Task<Replies<OrderItem>> FailMakeOrderItems( Order o, Replies<OrderItem> itemsResult ) =>
        Replies<OrderItem>.Fail( await CancelOrderRequest( o, null, itemsResult ) );
    async Task<Reply<OrderPlaceResponse>> HandleObjidOrder( Order order, List<ItemOrderGroup> groups )
    {
        if ((await MakeOrderGroups( order, groups )).OutFailure( out Replies<OrderLine> orderOption ))
            await CancelOrderRequest( order, null, orderOption );

        return (await ConfirmLocationOrders( orderOption.Enumerable ))
            .OutSuccess( out Reply<bool> confirmOption )
                ? await SendConfirmationEmailAndReturn( order, orderOption.Enumerable.ToList() )
                : await CancelOrderRequest( order, orderOption.Enumerable.ToList(), confirmOption );
    }
    async Task<Reply<OrderPlaceResponse>> HandleConflictedOrder( Order order, ReplyLine<ItemOrderGroup> groups )
    {
        if ((await CancelOrderRequest( order, null, Reply<bool>.Success( true ) ))
            .OutSuccess( out Reply<OrderPlaceResponse> cancelResult ))
            return cancelResult;

        List<Guid> unavailableItemIds = [];
        foreach ( Reply<ItemOrderGroup> o in groups.Options )
            if (!o.Succeeded)
                unavailableItemIds.Add( o.Data.Item.Id );
        return ConflictedOrder( unavailableItemIds );
    }

    // Make Order Groups
    async Task<Replies<OrderLine>> MakeOrderGroups( Order order, List<ItemOrderGroup> groupedItems )
    {
        Dictionary<OrderLocation, OrderLine> checkedLocations = [];

        if (!GetCheckedLocations( order, groupedItems, ref checkedLocations ))
            return CheckLocationsFail();

        SetGroupItemLocations( groupedItems, checkedLocations );

        if ((await InsertOrderLines( checkedLocations )).OutSuccess( out Reply<bool> insertResult ))
            return FailInsertOrderLines( insertResult );

        SetItemOrderGroupIds( groupedItems );

        return (await Save()).OutSuccess( out Reply<bool> saveOpt )
            ? Replies<OrderLine>.Success( checkedLocations.Values )
            : Replies<OrderLine>.Fail( saveOpt );
    }
    static bool GetCheckedLocations( Order order, List<ItemOrderGroup> groupedItems, ref Dictionary<OrderLocation, OrderLine> checkedLocations )
    {
        foreach ( ItemOrderGroup gi in groupedItems )
            if (!checkedLocations.ContainsKey( gi.Location ) &&
                !checkedLocations.TryAdd( gi.Location, new OrderLine( order.Id, gi.Location.Id ) ))
                return false;
        return true;
    }
    static void SetGroupItemLocations( List<ItemOrderGroup> groupedItems, Dictionary<OrderLocation, OrderLine> checkedLocations )
    {
        foreach ( ItemOrderGroup gi in groupedItems )
            gi.Line = checkedLocations[gi.Location];
    }
    async Task<Reply<bool>> InsertOrderLines( Dictionary<OrderLocation, OrderLine> checkedLocations ) =>
        await _repo.InsertOrderLines( checkedLocations.Values.ToList() );
    static void SetItemOrderGroupIds( List<ItemOrderGroup> groupedItems )
    {
        foreach ( ItemOrderGroup i in groupedItems )
            i.Item.OrderLineId = i.Line.Id;
    }

    // Location I/O
    async Task<Reply<OrderLocation>> FindNearestLocation( WorldGridPos shippingWorldGridPos, Guid productId, int productQuantity ) =>
        (await CheckLocations( shippingWorldGridPos, productId, productQuantity ))
        .OutSuccess( out Reply<OrderLocation> location )
            ? LocationFound( location.Data )
            : NoLocationFound( productId, productQuantity );
    async Task<Reply<OrderLocation>> CheckLocations( WorldGridPos shippingWorldGridPos, Guid productId, int productQuantity )
    {
        OrderLocation? bestLocation = null;
        foreach ( OrderLocation newLocation in _locationService.Cache.Locations )
            bestLocation = await CheckLocation( newLocation, bestLocation, shippingWorldGridPos, productId, productQuantity )
                ? newLocation
                : bestLocation;
        
        return bestLocation is null
            ? Reply<OrderLocation>.Failure()
            : Reply<OrderLocation>.Success( bestLocation );
    }
    async Task<bool> CheckLocation( OrderLocation location, OrderLocation? bestLocation, WorldGridPos shippingWorldGridPos, Guid productId, int productQuantity )
    {
        if ((await _locationService.CheckOrderStock( location.Id, productId, productQuantity ))
            .OutSuccess( out Reply<bool> stockResult ))
            return false;

        return location.WorldGridPos.HeuristicDistanceFrom( shippingWorldGridPos )
                       .IsLessThan( bestLocation?.WorldGridPos.HeuristicDistanceFrom( shippingWorldGridPos ) ?? HeuristicDistance.Max() );
    }
    async Task<Reply<bool>> ConfirmLocationOrders( IEnumerable<OrderLine> lines )
    {
        foreach ( OrderLine l in lines )
            if ((await _locationService.PlaceOrderLine( l ))
                .OutSuccess( out Reply<bool> result ))
                return FailedConfirmLocationOrders( result );
        return ConfirmedLocationOrders();
    }
    async Task<Reply<OrderPlaceResponse>> SendConfirmationEmailAndReturn( Order order, List<OrderLine> orderGroups )
    {
        string body =
            $@"<h2>Order Confirmation</h2>
            <p>Dear Customer,</p>
            <p>Your order with the following details has been successfully placed:</p>
            <p>Order ID: {order.Id}</p>
            <p>Order Date: {order.OrderDate}</p>
            <p>Total Price: {order.TotalPrice}</p>
            <p>Shipping Address: {order.ShippingWorldGridPos}</p>
            <p>Billing Address: {order.BillingWorldGridPos}</p>
            <p>Thank you for shopping with us!</p>";

        _emailSender.SendBasicEmail( order.CustomerEmail, "Order Confirmation", body );
        UpdateOrderStatus( order, orderGroups );

        return (await Save()).OutSuccess( out Reply<bool> saveResult )
            ? OrderPlaced( order )
            : await CancelOrderRequest( order, orderGroups, saveResult );
    }
    static void UpdateOrderStatus( Order order, List<OrderLine> orderGroups )
    {
        order.State = OrderState.Processed;
        foreach ( OrderLine g in orderGroups )
            g.State = OrderState.Processed;
    }

    // Save
    async Task<Reply<bool>> Save() =>
        await _repo.SaveAsync();

    // Returns Syntax Sugar
    static Reply<OrderPlaceResponse> FailCancelOrderLines( IReply originalError, Reply<bool> cancelResult ) =>
        Reply<OrderPlaceResponse>.Failure( $"{originalError.GetMessage()} And failed to cancel order lines. Please contact support. {cancelResult.GetMessage()}" );
    static Reply<OrderPlaceResponse> FailDeleteOrderData( IReply originalError, Reply<bool> deleteResult ) =>
        Reply<OrderPlaceResponse>.Failure( $"{originalError.GetMessage()} And failed to delete the order data : {deleteResult.GetMessage()}" );
    static Reply<OrderPlaceResponse> CancelOrderSuccess( IReply originalError ) =>
        Reply<OrderPlaceResponse>.Failure( originalError.GetMessage() );
    static Reply<OrderPlaceResponse> ConflictedOrder( List<Guid> unavailableItemIds ) =>
        Reply<OrderPlaceResponse>.Success( OrderPlaceResponse.Unavailable( unavailableItemIds ) );
    static Reply<ItemOrderGroup> FailedMakeOrderLocation() =>
        Reply<ItemOrderGroup>.Failure( "Failed to make order locations." );
    static Reply<ItemOrderGroup> MadeOrderLocation( OrderItem item, OrderLocation location ) =>
        Reply<ItemOrderGroup>.Success( ItemOrderGroup.With( item, location ) );
    static Reply<bool> NoLinesToCancel() =>
        Reply<bool>.Success( true );
    static Reply<bool> OrderLinesCancelled() =>
        Reply<bool>.Success( true );
    static Replies<OrderLine> CheckLocationsFail() =>
        Replies<OrderLine>.Fail( "Failed to create an order group." );
    static Replies<OrderLine> FailInsertOrderLines( Reply<bool> insert ) =>
        Replies<OrderLine>.Fail( insert.GetMessage() );
    static Reply<OrderLocation> LocationFound( OrderLocation bestLocation ) =>
        Reply<OrderLocation>.Success( bestLocation );
    static Reply<OrderLocation> NoLocationFound( Guid productId, int productQuantity ) =>
        Reply<OrderLocation>.Failure( $"No best location was found for product id: {productId} with quantity: {productQuantity}" );
    static Reply<bool> FailedConfirmLocationOrders( Reply<bool> result ) =>
        result;
    static Reply<bool> ConfirmedLocationOrders() =>
        Reply<bool>.Success( true );
    static Reply<OrderPlaceResponse> OrderPlaced( Order order ) =>
        Reply<OrderPlaceResponse>.Success( OrderPlaceResponse.Placed( order.Id ) );

    sealed class ItemOrderGroup
    {
        public readonly OrderItem Item = new();
        public readonly OrderLocation Location = new();
        public OrderLine Line = new();
        public ItemOrderGroup() { }
        ItemOrderGroup( OrderItem item, OrderLocation location )
        {
            Item = item;
            Location = location;
        }
        public static ItemOrderGroup With( OrderItem item, OrderLocation location ) =>
            new( item, location );
    }
}