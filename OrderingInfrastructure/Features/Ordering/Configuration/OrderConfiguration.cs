using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderingDomain.Orders.Base;

namespace OrderingInfrastructure.Features.Ordering.Configuration;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure( EntityTypeBuilder<Order> builder )
    {
        builder.HasKey( o => o.Id );
        builder.Property( o => o.UserId ).IsRequired().HasMaxLength( 450 );
        builder.ComplexProperty( o => o.Contact );
        builder.ComplexProperty( o => o.BillingAddress );
        builder.ComplexProperty( o => o.ShippingAddress );
        builder.ComplexProperty( o => o.Pricing );
        builder.Property( o => o.DatePlaced ).IsRequired();
        builder.Property( o => o.LastUpdate ).IsRequired();
        builder.Property( o => o.TotalQuantity ).IsRequired();
        builder.Property( o => o.Delayed ).IsRequired();
        builder.Property( o => o.Problem ).IsRequired();
        builder.Property( o => o.State ).IsRequired();

        builder.HasMany( o => o.OrderGroups )
            .WithOne( og => og.Order )
            .HasForeignKey( og => og.OrderId )
            .OnDelete( DeleteBehavior.Cascade );
    }
}