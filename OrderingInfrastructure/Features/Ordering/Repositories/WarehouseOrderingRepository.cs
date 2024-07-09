using Microsoft.Extensions.Logging;
using OrderingDomain.Orders.Base;
using OrderingDomain.ReplyTypes;

namespace OrderingInfrastructure.Features.Ordering.Repositories;

internal sealed class WarehouseOrderingRepository( OrderingDbContext database, ILogger<WarehouseOrderingRepository> logger )
    : DatabaseService<WarehouseOrderingRepository>( database, logger ), IWarehouseOrderingRepository
{
    public Task<Reply<bool>> InsertNewOrder( List<WarehouseOrderGroup> orderGroups ) => throw new NotImplementedException();
    public Task<Reply<List<WarehouseOrderGroup>>> GetNewOrders( Guid warehouseId ) => throw new NotImplementedException();
    public Task<Reply<bool>> RemoveOrderGroup( Guid orderId, Guid orderGroupId ) => throw new NotImplementedException();
}