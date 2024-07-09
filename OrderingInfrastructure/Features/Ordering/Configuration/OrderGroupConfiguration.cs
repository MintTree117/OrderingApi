using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderingDomain.Orders.Base;

namespace OrderingInfrastructure.Features.Ordering.Configuration;

public class OrderGroupConfiguration : IEntityTypeConfiguration<OrderGroup>
{
    public void Configure( EntityTypeBuilder<OrderGroup> builder )
    {
        builder.HasKey( static og => og.Id );
        builder.Property( static og => og.OrderId ).IsRequired();
        builder.Property( static og => og.WarehouseId ).IsRequired();
        builder.Property( static og => og.LastUpdated ).IsRequired();
        builder.Property( static og => og.State ).IsRequired();

        builder.HasMany( static og => og.OrderLines )
            .WithOne( static ol => ol.OrderGroup )
            .HasForeignKey( static ol => ol.OrderGroupId )
            .OnDelete( DeleteBehavior.Cascade );
    }
}