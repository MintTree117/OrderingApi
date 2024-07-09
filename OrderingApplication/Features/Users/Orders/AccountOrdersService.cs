using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Users.Utilities;
using OrderingDomain.Orders;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingInfrastructure.Features.Ordering.Repositories;

namespace OrderingApplication.Features.Users.Orders;

internal sealed class AccountOrdersService( UserManager<UserAccount> manager, ICustomerOrderingRepository repository )
{
    readonly UserManager<UserAccount> _manager = manager;
    readonly ICustomerOrderingRepository _repository = repository;

    internal async Task<Reply<AccountOrdersViewDto>> ViewPaginatedOrders( string userId, int page, int pageSize )
    {
        var userReply = await _manager.FindById( userId );
        if (!userReply)
            return Reply<AccountOrdersViewDto>.UserNotFound();

        var countTask = _repository.CountOrdersForUser( userId );
        var pageTask = _repository.GetPaginatedOrdersByUserId( userId, page, pageSize );

        List<Task> _dbTasks = [countTask, pageTask];
        await Task.WhenAll( _dbTasks );

        if (!countTask.Result)
            return Reply<AccountOrdersViewDto>.Failure( countTask.Result.GetMessage() );
        if (!pageTask.Result)
            return Reply<AccountOrdersViewDto>.Failure( pageTask.Result.GetMessage() );

        List<AccountOrderViewDto> dtos = [];
        foreach ( var order in pageTask.Result.Data )
            dtos.Add( AccountOrderViewDto.FromModel( order ) );
        
        AccountOrdersViewDto response = new( countTask.Result.Data, dtos );
        return Reply<AccountOrdersViewDto>.Success( response );
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