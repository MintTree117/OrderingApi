using Microsoft.EntityFrameworkCore;
using OrderingDomain.Orders;

namespace OrderingInfrastructure.Features.Ordering;

internal sealed class OrderingDbContext( DbContextOptions<OrderingDbContext> options ) 
    : DbContext( options )
{
    protected override void OnModelCreating( ModelBuilder modelBuilder )
    {
        // Order configuration
        modelBuilder.Entity<Order>( static entity => {
            entity.HasKey( static o => o.Id );
            
            entity.Property( static o => o.UserId )
                .HasMaxLength( 50 );
            entity.Property( static o => o.CustomerName )
                .IsRequired()
                .HasMaxLength( 100 );
            entity.Property( static o => o.CustomerEmail )
                .IsRequired()
                .HasMaxLength( 100 );
            entity.Property( static o => o.CustomerPhone )
                .HasMaxLength( 15 );
            entity.Property( static o => o.DatePlaced )
                .IsRequired();
            entity.Property( static o => o.LastUpdate )
                .IsRequired();
            entity.Property( static o => o.TotalPrice )
                .IsRequired()
                .HasColumnType( "decimal(18,2)" );
            entity.Property( static o => o.TotalQuantity )
                .IsRequired();
            entity.Property( static o => o.Delayed )
                .IsRequired();
            entity.Property( static o => o.Problem )
                .IsRequired();
            entity.Property( static o => o.State )
                .IsRequired();

            // Configuring owned entity
            entity.OwnsOne( static o => o.OrderAddress, static oa => {
                oa.WithOwner().HasForeignKey( static oa => oa.OrderId );
                oa.Property( static a => a.BillingAddressName ).IsRequired().HasMaxLength( 100 );
                oa.Property( static a => a.BillingPosX ).IsRequired();
                oa.Property( static a => a.BillingPosY ).IsRequired();
                oa.Property( static a => a.ShippingAddressName ).IsRequired().HasMaxLength( 100 );
                oa.Property( static a => a.ShippingPosX ).IsRequired();
                oa.Property( static a => a.ShippingPosY ).IsRequired();
                oa.HasKey( static a => a.Id );
            } );

            // Configuring relationships
            entity.HasMany( static o => o.OrderGroups )
                .WithOne( static og => og.Order)
                .HasForeignKey( static og => og.OrderId );
        } );

        // OrderGroup configuration
        modelBuilder.Entity<OrderGroup>( static entity => {
            entity.HasKey( static og => og.Id );
            entity.Property( static og => og.LastUpdated )
                .IsRequired();
            entity.Property( static og => og.State )
                .IsRequired();
            entity.HasMany( static og => og.OrderLines )
                .WithOne( static ol => ol.OrderGroup )
                .HasForeignKey( static ol => ol.OrderGroupId );
        } );

        // OrderLine configuration
        modelBuilder.Entity<OrderLine>( static entity => {
            entity.HasKey( static ol => ol.Id );
            entity.Property( static ol => ol.UnitPrice )
                .IsRequired()
                .HasColumnType( "decimal(18,2)" );
            entity.Property( static ol => ol.Discount )
                .IsRequired()
                .HasColumnType( "decimal(18,2)" );
            entity.Property( static ol => ol.Tax )
                .IsRequired()
                .HasColumnType( "decimal(18,2)" );
            entity.Property( static ol => ol.Quantity )
                .IsRequired();
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