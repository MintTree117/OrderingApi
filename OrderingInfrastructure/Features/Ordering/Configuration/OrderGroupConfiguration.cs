using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderingDomain.Orders.Base;

namespace OrderingInfrastructure.Features.Ordering.Configuration;

public class OrderGroupConfiguration : IEntityTypeConfiguration<OrderGroup>
{
    public void Configure( EntityTypeBuilder<OrderGroup> builder )
    {
        builder.HasKey( og => og.Id );
        builder.Property( og => og.OrderId ).IsRequired();
        builder.Property( og => og.WarehouseId ).IsRequired();
        builder.Property( og => og.LastUpdated ).IsRequired();
        builder.Property( og => og.State ).IsRequired();

        builder.HasMany( og => og.OrderLines )
            .WithOne( ol => ol.OrderGroup )
            .HasForeignKey( ol => ol.OrderGroupId )
            .OnDelete( DeleteBehavior.Cascade );
    }
}