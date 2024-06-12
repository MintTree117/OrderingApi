using Microsoft.EntityFrameworkCore;
using OrderingDomain.Orders;

namespace OrderingInfrastructure.Features.Ordering;

internal sealed class OrderingDbContext( DbContextOptions<OrderingDbContext> options ) 
    : DbContext( options )
{
    public required DbSet<OrderStateDelayTime> DelayTimes { get; init; }
    public required DbSet<OrderStateExpireTime> ExpireTimes { get; init; }

    public required DbSet<OrderProblem> OrderProblems { get; init; }
    public required DbSet<OrderLine> PendingCancelOrderLines { get; init; }
    public required DbSet<Order> ActiveOrders { get; init; }
    public required DbSet<OrderLine> ActiveOrderLines { get; init; }
    public required DbSet<OrderItem> ActiveOrderItems { get; init; }
    public required DbSet<Order> InActiveOrders { get; init; }
    public required DbSet<OrderLine> InActiveOrderGroups { get; init; }
    public required DbSet<OrderItem> InActiveOrderItems { get; init; }
    public required DbSet<OrderLocation> WarehouseLocations { get; init; }
}