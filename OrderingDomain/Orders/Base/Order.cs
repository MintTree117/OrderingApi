using OrderingDomain.Orders.Meta;
using OrderingDomain.ValueTypes;

namespace OrderingDomain.Orders.Base;

public sealed class Order
{
    public Guid Id { get; set; } = Guid.Empty;
    public string? UserId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public OrderAddress OrderAddress { get; set; } = null!;
    public DateTime DatePlaced { get; set; }
    public DateTime LastUpdate { get; set; }
    public decimal TotalPrice { get; set; }
    public int TotalQuantity { get; set; }
    public bool Delayed { get; set; }
    public bool Problem { get; set; }
    public OrderState State { get; set; } = OrderState.Placed;
    public List<OrderGroup> OrderGroups { get; set; } = [];

    public static Order New(
        string? userId,
        Contact contact,
        Address billingAddress,
        Address shippingAddress )
    {
        Guid id = Guid.NewGuid();
        Order o = new() {
            Id = id,
            UserId = userId,
            CustomerName = contact.Name,
            CustomerEmail = contact.Email,
            CustomerPhone = contact.Phone,
            OrderAddress = OrderAddress.From( id, billingAddress, shippingAddress ),
            DatePlaced = DateTime.Now,
            LastUpdate = DateTime.Now
        };

        return o;
    }
}