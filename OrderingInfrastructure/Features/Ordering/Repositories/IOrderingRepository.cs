using OrderingDomain.Orders.Base;
using OrderingDomain.Orders.Meta;
using OrderingDomain.ReplyTypes;

namespace OrderingInfrastructure.Features.Ordering.Repositories;

public interface IOrderingRepository : IEfCoreRepository
{
    Task<Reply<bool>> InsertOrder( Order order );
    Task<Reply<bool>> InsertOrderGroups( IEnumerable<OrderGroup> orderGroups );
    Task<Reply<bool>> InsertOrderLines( IEnumerable<OrderLine> orderLines );
    Task<Reply<bool>> InsertOrderProblem( OrderProblem problem );
    Task<Reply<bool>> DeleteOrderData( Guid orderId );
    Task<Reply<Order>> GetOrderById( Guid orderId );
    Task<Reply<OrderGroup>> GetOrderGroupById( Guid orderGroupId );
    Task<Replies<OrderGroup>> GetOrderGroupsForOrder( Guid orderId );
}