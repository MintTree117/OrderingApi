using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderingDomain.Orders.Base;

namespace OrderingInfrastructure.Features.Ordering.Configuration;

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure( EntityTypeBuilder<OrderLine> builder )
    {
        builder.HasKey( static ol => ol.Id );
        builder.Property( static ol => ol.OrderId ).IsRequired();
        builder.Property( static ol => ol.OrderGroupId ).IsRequired();
        builder.Property( static ol => ol.WarehouseId ).IsRequired();
        builder.Property( static ol => ol.UnitId ).IsRequired();
        builder.Property( static ol => ol.UnitPrice ).IsRequired().HasColumnType( "decimal(18,2)" );
        builder.Property( static ol => ol.Discount ).IsRequired().HasColumnType( "decimal(18,2)" );
        builder.Property( static ol => ol.Tax ).IsRequired().HasColumnType( "decimal(18,2)" );
        builder.Property( static ol => ol.Quantity ).IsRequired();
    }
}