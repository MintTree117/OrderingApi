using OrderingDomain.Orders.Meta;
using OrderingDomain.ValueTypes;

namespace OrderingDomain.Orders.Base;

public class Order
{
    public Guid Id { get; set; } = Guid.Empty;
    public string? UserId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string BillingAddressName { get; set; } = string.Empty;
    public int BillingPosX { get; set; }
    public int BillingPosY { get; set; }
    public string ShippingAddressName { get; set; } = string.Empty;
    public int ShippingPosX { get; set; }
    public int ShippingPosY { get; set; }
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
        Address shippingAddress,
        Address billingAddress )
    {
        Guid id = Guid.NewGuid();
        Order o = new() {
            Id = id,
            UserId = userId,
            CustomerName = contact.Name,
            CustomerEmail = contact.Email,
            CustomerPhone = contact.Phone,
            ShippingAddressName = shippingAddress.Name,
            ShippingPosX = shippingAddress.PosX,
            ShippingPosY = shippingAddress.PosY,
            BillingAddressName = billingAddress.Name,
            BillingPosX = billingAddress.PosX,
            BillingPosY = billingAddress.PosY,
            DatePlaced = DateTime.Now,
            LastUpdate = DateTime.Now
        };

        return o;
    }

}