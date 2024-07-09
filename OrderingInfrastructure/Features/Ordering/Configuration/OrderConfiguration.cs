using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderingDomain.Orders.Base;

namespace OrderingInfrastructure.Features.Ordering.Configuration;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure( EntityTypeBuilder<Order> builder )
    {
        builder.HasKey( static o => o.Id );
        builder.Property( static o => o.UserId ).IsRequired().HasMaxLength( 450 );
        //builder.ComplexProperty( o => o.Contact );
        builder.ComplexProperty( static o => o.BillingAddress );
        builder.ComplexProperty( static o => o.ShippingAddress );
        //builder.ComplexProperty( o => o.Pricing );
        builder.Property( static o => o.DatePlaced ).IsRequired();
        builder.Property( static o => o.LastUpdate ).IsRequired();
        builder.Property( static o => o.TotalQuantity ).IsRequired();
        builder.Property( static o => o.Delayed ).IsRequired();
        builder.Property( static o => o.Problem ).IsRequired();
        builder.Property( static o => o.State ).IsRequired();

        builder.HasMany( static o => o.OrderGroups )
            .WithOne( static og => og.Order )
            .HasForeignKey( static og => og.OrderId )
            .OnDelete( DeleteBehavior.Cascade );
    }
}