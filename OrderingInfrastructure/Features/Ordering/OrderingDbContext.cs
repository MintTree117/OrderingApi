using Microsoft.EntityFrameworkCore;
using OrderingDomain.Orders.Base;
using OrderingDomain.Orders.Meta;
namespace OrderingInfrastructure.Features.Ordering;

internal sealed class OrderingDbContext( DbContextOptions<OrderingDbContext> options ) 
    : DbContext( options )
{
    protected override void OnModelCreating( ModelBuilder modelBuilder )
    {
        modelBuilder.Entity<Order>().HasKey( static o => o.Id );
        modelBuilder.Entity<Order>().OwnsOne( static o => o.OrderAddress, static oa => {
            oa.WithOwner().HasForeignKey( static oa => oa.OrderId );
            oa.Property( static a => a.BillingAddressName ).IsRequired();
            oa.Property( static a => a.BillingPosX ).IsRequired();
            oa.Property( static a => a.BillingPosY ).IsRequired();
            oa.Property( static a => a.ShippingAddressName ).IsRequired();
            oa.Property( static a => a.ShippingPosX ).IsRequired();
            oa.Property( static a => a.ShippingPosY ).IsRequired();
            oa.HasKey( static a => a.Id );
        } );
    }

    public required DbSet<Order> Orders { get; init; }
    public required DbSet<OrderGroup> OrderGroups { get; init; }
    public required DbSet<OrderLine> OrderLines { get; init; }
    public required DbSet<OrderAddress> OrderAddresses { get; init; }
    public required DbSet<OrderProblem> OrderProblems { get; init; }
    public required DbSet<Warehouse> WarehouseLocations { get; init; }
    public required DbSet<WarehouseOrderGroup> WarehouseOrderGroups { get; init; }
}