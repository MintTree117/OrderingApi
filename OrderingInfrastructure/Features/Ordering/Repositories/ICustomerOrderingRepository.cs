using OrderingDomain.Orders.Base;
using OrderingDomain.Orders.Meta;
using OrderingDomain.ReplyTypes;

namespace OrderingInfrastructure.Features.Ordering.Repositories;

public interface ICustomerOrderingRepository : IEfCoreRepository
{
    Task<Reply<bool>> InsertOrder( Order order );
    Task<Reply<bool>> InsertOrderProblem( OrderProblem problem );
    Task<Reply<bool>> DeleteOrderData( Guid orderId );
    Task<Reply<Order>> GetOrderById( Guid orderId );
    Task<Reply<int>> CountOrdersForUser( string userId );
    Task<Reply<List<Order>>> GetPaginatedOrdersByUserId( string userId, int page, int pageSize );
    Task<Reply<OrderGroup>> GetOrderGroupById( Guid orderGroupId );
    Task<Reply<List<OrderGroup>>> GetOrderGroupsForOrder( Guid orderId );
}