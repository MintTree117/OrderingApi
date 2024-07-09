using OrderingDomain.Orders;
using OrderingDomain.ReplyTypes;

namespace OrderingInfrastructure.Features.Ordering.Repositories;

public interface IWarehouseOrderingRepository
{
    Task<Reply<bool>> InsertNewOrder( List<WarehouseOrderGroup> orderGroups );
    Task<Reply<List<WarehouseOrderGroup>>> GetNewOrders( Guid warehouseId );
    Task<Reply<bool>> RemoveOrderGroup( Guid orderId, Guid orderGroupId );
}