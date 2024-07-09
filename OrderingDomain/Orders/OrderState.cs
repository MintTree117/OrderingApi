namespace OrderingDomain.Orders;

public enum OrderState
{
    Placed,
    Fulfilling,
    Shipping,
    Delivered,
    Cancelled
}