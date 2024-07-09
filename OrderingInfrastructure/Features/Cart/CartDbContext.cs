using Microsoft.EntityFrameworkCore;
using OrderingDomain.Cart;

namespace OrderingInfrastructure.Features.Cart;

internal sealed class CartDbContext( DbContextOptions<CartDbContext> options )
    : DbContext( options )
{
    protected override void OnModelCreating( ModelBuilder modelBuilder )
    {
        modelBuilder.Entity<CartItem>( static entity => {
            entity.HasKey( static ci => ci.Id );
            entity.Property( static ci => ci.UserId )
                .HasMaxLength( 128 );
            entity.Property( static ci => ci.ProductId )
                .IsRequired();
            entity.Property( static ci => ci.Quantity )
                .IsRequired();
        } );
    }
    
    public required DbSet<CartItem> CartItems { get; init; }
}