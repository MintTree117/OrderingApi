using Microsoft.Extensions.Logging;
using OrderingDomain.Orders;
using OrderingDomain.ReplyTypes;

namespace OrderingInfrastructure.Features.Ordering.Repositories;

internal sealed class WarehouseOrderingRepository( OrderingDbContext database, ILogger<WarehouseOrderingRepository> logger )
    : DatabaseService<WarehouseOrderingRepository>( database, logger ), IWarehouseOrderingRepository
{
    readonly OrderingDbContext _database = database;
    
    public async Task<Reply<bool>> InsertNewOrder( List<WarehouseOrderGroup> orderGroups )
    {
        try
        {
            await _database.AddRangeAsync( orderGroups );
            return IReply.Success();
        }
        catch ( Exception e )
        {
            return ProcessDbException<bool>( e );
        }
    }
    public Task<Reply<List<WarehouseOrderGroup>>> GetNewOrders( Guid warehouseId ) => throw new NotImplementedException();
    public Task<Reply<bool>> RemoveOrderGroup( Guid orderId, Guid orderGroupId ) => throw new NotImplementedException();
}