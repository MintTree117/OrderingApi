using Microsoft.EntityFrameworkCore;
using OrderingDomain.Orders.Base;
using OrderingDomain.Orders.Meta;
using OrderingInfrastructure.Features.Ordering.Configuration;

namespace OrderingInfrastructure.Features.Ordering;

internal sealed class OrderingDbContext( DbContextOptions<OrderingDbContext> options ) 
    : DbContext( options )
{
    protected override void OnModelCreating( ModelBuilder builder )
    {
        builder.ApplyConfiguration( new OrderConfiguration() );
        builder.ApplyConfiguration( new OrderGroupConfiguration() );
        builder.ApplyConfiguration( new OrderLineConfiguration() );

        base.OnModelCreating( builder );
    }
    
    public required DbSet<Order> Orders { get; init; }
    public required DbSet<OrderGroup> OrderGroups { get; init; }
    public required DbSet<OrderLine> OrderLines { get; init; }
    public required DbSet<OrderProblem> OrderProblems { get; init; }
    public required DbSet<Warehouse> WarehouseLocations { get; init; }
}