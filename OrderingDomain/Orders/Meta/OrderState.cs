namespace OrderingDomain.Orders.Meta;

public enum OrderState
{
    Placed,
    Fulfilling,
    Shipping,
    Delivered,
    Cancelled
}