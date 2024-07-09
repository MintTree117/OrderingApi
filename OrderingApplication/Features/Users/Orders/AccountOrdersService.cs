using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Users.Utilities;
using OrderingDomain.Orders.Base;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingInfrastructure.Features.Ordering.Repositories;

namespace OrderingApplication.Features.Users.Orders;

internal sealed class AccountOrdersService( UserManager<UserAccount> manager, ICustomerOrderingRepository repository )
{
    readonly UserManager<UserAccount> _manager = manager;
    readonly ICustomerOrderingRepository _repository = repository;

    internal async Task<Reply<List<AccountOrdersViewDto>>> ViewPaginatedOrders( string userId, int page, int pageSize )
    {
        var userReply = await _manager.FindById( userId );
        if (!userReply)
            return Reply<List<AccountOrdersViewDto>>.UserNotFound();

        var ordersReply = await _repository.GetPaginatedOrdersByUserId( userId, page, pageSize );
        if (!ordersReply)
            return Reply<List<AccountOrdersViewDto>>.Failure( ordersReply.GetMessage() );

        List<AccountOrdersViewDto> dtos = [];
        foreach ( var order in ordersReply.Data )
            dtos.Add( AccountOrdersViewDto.FromModel( order ) );
        return Reply<List<AccountOrdersViewDto>>.Success( dtos );
    }
    internal async Task<Reply<Order>> ViewOrderDetails( string userId, Guid orderId )
    {
        var userReply = await _manager.FindById( userId );
        if (!userReply)
            return Reply<Order>.UserNotFound();

        var orderReply = await _repository.GetOrderById( orderId );
        return orderReply;
    }
}