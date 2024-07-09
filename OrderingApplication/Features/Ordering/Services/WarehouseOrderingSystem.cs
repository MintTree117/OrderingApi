using OrderingApplication.Features.Ordering.Dtos;
using OrderingDomain.Orders.Base;
using OrderingDomain.Orders.Meta;
using OrderingDomain.ReplyTypes;
using OrderingInfrastructure.Email;
using OrderingInfrastructure.Features.Ordering.Repositories;

namespace OrderingApplication.Features.Ordering.Services;

internal sealed class WarehouseOrderingSystem( IWarehouseOrderingRepository warehouseRepository, ICustomerOrderingRepository customerRepository, IEmailSender emailSender )
{
    readonly IWarehouseOrderingRepository _warehouseRepository = warehouseRepository;
    readonly ICustomerOrderingRepository _customerRepository = customerRepository;
    readonly IEmailSender _emailSender = emailSender;

    internal async Task<Reply<List<WarehouseOrderGroup>>> GetNewWarehouseOrderGroups( Guid warehouseId )
    {
        var dbReply = await _warehouseRepository.GetNewOrders( warehouseId );
        return dbReply;
    }
    internal async Task<Reply<bool>> InsertNewWarehouseOrderGroups( Order order )
    {
        var warehouseGroups = WarehouseOrderGroup.FromOrder( order );
        var dbReply = await _warehouseRepository.InsertNewOrder( warehouseGroups );
        return dbReply;
    }
    internal async Task<Reply<bool>> UpdateOrder( OrderUpdateRequest update )
    {
        var orderReply = await _customerRepository.GetOrderById( update.OrderId );
        if (!orderReply)
            return IReply.NotFound( "Order not found." );

        var orderGroupsReply = await _customerRepository.GetOrderGroupsForOrder( update.OrderId );
        if (!orderGroupsReply)
            return IReply.NotFound( "OrderGroup not found." );

        string email = orderReply.Data.CustomerEmail;
        var groupUpdate = await HandleGroupUpdate( email, orderGroupsReply.Enumerable, update.OrderGroupId, update.OrderState );
        if (!groupUpdate)
            return IReply.Fail( groupUpdate.GetMessage() );

        var orderUpdate = await HandleOrderUpdate( email, orderReply.Data, orderGroupsReply.Enumerable, update.OrderState );
        if (!orderUpdate)
            return IReply.Fail( orderUpdate.GetMessage() );
        
        var dbReply = await _customerRepository.SaveAsync();
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

        if (state == OrderState.Delivered)
            await _warehouseRepository.RemoveOrderGroup( group.OrderId, group.Id );
        
        group.Update( state );
        return await _customerRepository.SaveAsync();
    }
    async Task<Reply<bool>> HandleOrderUpdate( string emailAddress, Order order, IEnumerable<OrderGroup> groups, OrderState state )
    {
        bool same = groups.All( g => g.State == state );
        if (!same)
            return IReply.Success();
        
        string email = OrderingEmailUtility.GenerateOrderUpdateEmail( order, state );
        _emailSender.SendHtmlEmail( emailAddress, "Order Updated", email );
            
        order.State = state;
        return await _customerRepository.SaveAsync();
    }
}