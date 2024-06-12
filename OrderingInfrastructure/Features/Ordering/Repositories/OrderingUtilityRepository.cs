using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderingDomain.Orders;
using OrderingDomain.ReplyTypes;

namespace OrderingInfrastructure.Features.Ordering.Repositories;

internal sealed class OrderingUtilityRepository( OrderingDbContext database, ILogger<OrderingUtilityRepository> logger ) 
    : DatabaseService<OrderingUtilityRepository>( database, logger ), IOrderingUtilityRepository
{
    readonly OrderingDbContext _database = database;
    
    public async Task<Reply<bool>> InsertOrderProblem( OrderProblem problem )
    {
        try {
            await _database.OrderProblems.AddAsync( problem );
            return await SaveAsync();
        }
        catch ( Exception e ) {
            return HandleDbException<bool>( e );
        }
    }
    public async Task<Reply<bool>> InsertPendingCancelLine( OrderLine line )
    {
        try {
            await _database.PendingCancelOrderLines.AddAsync( line );
            return await SaveAsync();
        }
        catch ( Exception e ) {
            return HandleDbException<bool>( e );
        }
    }
    public async Task<Reply<bool>> DeletePendingDeleteLine( OrderLine line )
    {
        try {
            _database.PendingCancelOrderLines.Remove( line );
            return await SaveAsync();
        }
        catch ( Exception e ) {
            return HandleDbException<bool>( e );
        }
    }
    public async Task<Replies<OrderStateDelayTime>> GetDelayTimes()
    {
        try {
            return Replies<OrderStateDelayTime>.Success(
                await _database.DelayTimes.ToListAsync() );
        }
        catch ( Exception e ) {
            return HandleDbExceptionOpts<OrderStateDelayTime>( e );
        }
    }
    public async Task<Replies<OrderStateExpireTime>> GetExpiryTimes()
    {
        try {
            return Replies<OrderStateExpireTime>.Success(
                await _database.ExpireTimes.ToListAsync() );
        }
        catch ( Exception e ) {
            return HandleDbExceptionOpts<OrderStateExpireTime>( e );
        }
    }
    public async Task<Replies<OrderLine>> GetTopUnhandledDelayedOrderLines( int amount, int checkHours )
    {
        try {
            return Replies<OrderLine>.Success(
                await _database.ActiveOrderLines.Where(
                    o => !o.Delayed && DateTime.Now - o.LastUpdate > TimeSpan.FromHours( checkHours ) ).ToListAsync() );
        }
        catch ( Exception e ) {
            return HandleDbExceptionOpts<OrderLine>( e );
        }
    }
    public async Task<Replies<OrderLine>> GetTopUnhandledExpiredOrderLines( int amount, int checkHours )
    {
        try {
            return Replies<OrderLine>.Success(
                await _database.ActiveOrderLines.Where(
                    o => !o.Problem && DateTime.Now - o.LastUpdate > TimeSpan.FromHours( checkHours ) ).ToListAsync() );
        }
        catch ( Exception e ) {
            return HandleDbExceptionOpts<OrderLine>( e );
        }
    }
    public async Task<Replies<OrderLine>> GetPendingCancelLines()
    {
        try {
            return Replies<OrderLine>.Success(
                await _database.ActiveOrderLines.Where( static o => o.Problem ).ToListAsync() );
        }
        catch ( Exception e ) {
            return HandleDbExceptionOpts<OrderLine>( e );
        }
    }
}