using OrderingDomain.Cart;

namespace OrderingApplication.Features.Cart;

internal readonly record struct CartItemDto(
    Guid ProductId,
    int Quantity )
{
    public static CartItemDto New( Guid productId, int quantity ) =>
        new( productId, quantity );
    public CartItem ToModel( string userId ) =>
        new( userId, ProductId, Quantity );
}