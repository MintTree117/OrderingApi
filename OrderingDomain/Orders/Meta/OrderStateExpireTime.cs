namespace OrderingDomain.Orders.Meta;

public sealed class OrderStateExpireTime
{
    public Guid Id { get; set; }
    public OrderState State { get; set; }
    public TimeSpan ExpiryTime { get; set; }
}