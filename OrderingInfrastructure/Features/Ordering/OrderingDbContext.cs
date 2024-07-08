using Microsoft.EntityFrameworkCore;
using OrderingDomain.Orders.Base;
using OrderingDomain.Orders.Meta;

namespace OrderingInfrastructure.Features.Ordering;

internal sealed class OrderingDbContext( DbContextOptions<OrderingDbContext> options ) 
    : DbContext( options )
{
    public required DbSet<OrderStateDelayTime> DelayTimes { get; init; }
    public required DbSet<OrderStateExpireTime> ExpireTimes { get; init; }
    
    public required DbSet<Order> Orders { get; init; }
    public required DbSet<OrderGroup> OrderGroups { get; init; }
    public required DbSet<OrderLine> OrderLines { get; init; }
    public required DbSet<OrderProblem> OrderProblems { get; init; }
    public required DbSet<Warehouse> WarehouseLocations { get; init; }
}