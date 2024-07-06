using Microsoft.EntityFrameworkCore;
using OrderingDomain.Cart;
using OrderingDomain.ReplyTypes;

namespace OrderingInfrastructure.Features.Cart;

internal sealed class CartRepository( CartDbContext database ) : ICartRepository
{
    readonly CartDbContext _database = database;

    public async Task<Replies<CartItem>> PostGet( string userId, List<CartItem> itemsFromClient )
    {
        IQueryable<CartItem> items = _database.CartItems; // because of compiler warning "where" ambiguity error with IEnumerable
        var existingCartItems = await items
            .Where( c => c.UserId == userId )
            .ToListAsync();

        foreach ( var newItem in itemsFromClient )
        {
            var existing = existingCartItems
                .FirstOrDefault( c => c.ProductId == newItem.ProductId );
            
            if (existing is not null)
            {
                existing.Quantity = newItem.Quantity;
                _database.CartItems.Update( existing );
            }
            else
            {
                newItem.UserId = userId;
                existingCartItems.Add( newItem );
                _database.CartItems.Add( newItem );
            }
        }
        
        await _database.SaveChangesAsync();
        return Replies<CartItem>.Success( existingCartItems );
    }
    public async Task<Reply<bool>> Clear( string userId )
    {
        var items = _database.CartItems
            .Where( c => c.UserId == userId );
        
        _database.CartItems.RemoveRange( items );
        var saveResult = await _database.SaveChangesAsync();
        return saveResult > 0
            ? IReply.Success()
            : IReply.ServerError();
    }
}