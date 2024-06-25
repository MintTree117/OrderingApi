using Microsoft.EntityFrameworkCore;
using OrderingDomain.Cart;

namespace OrderingInfrastructure.Features.Cart;

internal sealed class CartDbContext( DbContextOptions<CartDbContext> options )
    : DbContext( options )
{
    public DbSet<CartItem> CartItems { get; set; } = default!;
}