namespace OrderingDomain.Cart;

public sealed class CartItem
{
    public CartItem() { }

    public CartItem( string userId, Guid productId, int quantity )
    {
        UserId = userId;
        ProductId = productId;
        Quantity = quantity;
    }

    public static CartItem WithoutUserId( Guid productId, int quantity ) => 
        new( string.Empty, productId, quantity );

    public Guid Id { get; set; } = Guid.Empty;
    public string UserId { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}