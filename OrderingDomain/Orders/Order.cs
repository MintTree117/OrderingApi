using OrderingDomain.ValueTypes;

namespace OrderingDomain.Orders;

public sealed class Order
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid CustomerId { get; set; } = Guid.Empty;
    public Guid OrderGroupId { get; set; } = Guid.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public WorldGridPos ShippingWorldGridPos { get; set; }
    public WorldGridPos BillingWorldGridPos { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public DateTime LastUpdate { get; set; } = DateTime.Now;
    public Pricing Pricing { get; set; }
    public decimal TotalPrice { get; set; }
    public int TotalQuantity { get; set; }
    public OrderState State { get; set; } = OrderState.Processing;
    public bool Delayed { get; set; }
    public bool Problem { get; set; }
}