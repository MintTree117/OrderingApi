using OrderingDomain.Cart;
using OrderingDomain.ReplyTypes;

namespace OrderingInfrastructure.Features.Cart;

public interface ICartRepository
{
    Task<Replies<CartItem>> PostGet( string userId, List<CartItem> itemsFromClient );
    Task<Reply<bool>> Add( string userId, Guid productId );
    Task<Reply<bool>> Update( string userId, Guid productId, int quantity );
    Task<Reply<bool>> UpdateBulk( string userId, List<CartItem> itemsFromClient );
    Task<Reply<bool>> Delete( string userId, Guid productId );
    Task<Reply<bool>> Clear( string userId );
}