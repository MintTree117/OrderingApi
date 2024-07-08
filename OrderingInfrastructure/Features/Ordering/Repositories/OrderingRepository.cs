using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using OrderingDomain.Orders.Base;
using OrderingDomain.Orders.Meta;
using OrderingDomain.ReplyTypes;

namespace OrderingInfrastructure.Features.Ordering.Repositories;

internal sealed class OrderingRepository( OrderingDbContext database, ILogger<OrderingRepository> logger ) 
    : DatabaseService<OrderingRepository>( database, logger ), IOrderingRepository
{
    readonly OrderingDbContext _database = database;

    public async Task<Reply<bool>> InsertOrder( Order order )
    {
        try
        {
            await _database.Orders.AddAsync( order );
            return await SaveAsync();
        }
        catch ( Exception e )
        {
            return ProcessDbException<bool>( e );
        }
    }
    public async Task<Reply<bool>> InsertOrderGroups( IEnumerable<OrderGroup> orderGroups )
    {
        try
        {
            await _database.AddRangeAsync( orderGroups );
            return await SaveAsync();
        }
        catch ( Exception e )
        {
            return ProcessDbException<bool>( e );
        }
    }
    public async Task<Reply<bool>> InsertOrderLines( IEnumerable<OrderLine> orderLines )
    {
        try
        {
            await _database.AddRangeAsync( orderLines );
            return await SaveAsync();
        }
        catch ( Exception e )
        {
            return ProcessDbException<bool>( e );
        }
    }
    public async Task<Reply<bool>> InsertOrderProblem( OrderProblem problem )
    {
        try
        {
            EntityEntry<OrderProblem> entry = await _database.OrderProblems.AddAsync( problem );
            if (entry.State != EntityState.Added)
                return IReply.ServerError();
            return await SaveAsync();
        }
        catch ( Exception e )
        {
            return ProcessDbException<bool>( e );
        }
    }
    public async Task<Reply<bool>> DeleteOrderData( Guid orderId )
    {
        try
        {
            Order? order = await _database.Orders.FirstOrDefaultAsync( o => o.Id == orderId );
            List<OrderGroup> orderGroups = await _database.OrderGroups.Where( i => i.OrderId == orderId ).ToListAsync();
            List<OrderLine> orderLines = await _database.OrderLines.Where( l => l.OrderId == orderId ).ToListAsync();

            _database.OrderLines.RemoveRange( orderLines );
            _database.OrderGroups.RemoveRange( orderGroups );
            if (order is not null)
                _database.Orders.Remove( order );

            return await SaveAsync();
        }
        catch ( Exception e )
        {
            return ProcessDbException<bool>( e );
        }
    }
    public async Task<Reply<Order>> GetOrderById( Guid orderId )
    {
        try
        {
            Order? order = await _database.Orders.FirstOrDefaultAsync( o => o.Id == orderId );
            return order is not null
                ? Reply<Order>.Success( order )
                : Reply<Order>.Failure( $"Order {orderId} not found in db." );
        }
        catch ( Exception e )
        {
            return ProcessDbException<Order>( e );
        }
    }
    public async Task<Reply<OrderGroup>> GetOrderGroupById( Guid orderGroupId )
    {
        try
        {
            OrderGroup? orderGroup = await _database.OrderGroups.FirstOrDefaultAsync( o => o.Id == orderGroupId );
            return orderGroup is not null
                ? Reply<OrderGroup>.Success( orderGroup )
                : Reply<OrderGroup>.Failure( $"Order {orderGroupId} not found in db." );
        }
        catch ( Exception e )
        {
            return ProcessDbException<OrderGroup>( e );
        }
    }
    public async Task<Replies<OrderGroup>> GetOrderGroupsForOrder( Guid orderId )
    {
        try
        {
            var orderGroups = await _database.OrderGroups.Where( o => o.Id == orderId ).ToListAsync();
            return Replies<OrderGroup>.Success( orderGroups );
        }
        catch ( Exception e )
        {
            return ProcessDbExceptionReplies<OrderGroup>( e );
        }
    }
}