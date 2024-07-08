using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderingDomain.Orders.Base;

namespace OrderingInfrastructure.Features.Ordering.Configuration;

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure( EntityTypeBuilder<OrderLine> builder )
    {
        builder.HasKey( ol => ol.Id );
        builder.Property( ol => ol.OrderId ).IsRequired();
        builder.Property( ol => ol.OrderGroupId ).IsRequired();
        builder.Property( ol => ol.WarehouseId ).IsRequired();
        builder.Property( ol => ol.UnitId ).IsRequired();
        builder.Property( ol => ol.UnitPrice ).IsRequired().HasColumnType( "decimal(18,2)" );
        builder.Property( ol => ol.Discount ).IsRequired().HasColumnType( "decimal(18,2)" );
        builder.Property( ol => ol.Tax ).IsRequired().HasColumnType( "decimal(18,2)" );
        builder.Property( ol => ol.Quantity ).IsRequired();
    }
}