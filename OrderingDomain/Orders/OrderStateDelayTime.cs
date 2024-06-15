namespace OrderingDomain.Orders;

public class OrderStateDelayTime
{
    public Guid Id { get; set; }
    public OrderState State { get; set; }
    public TimeSpan DelayTime { get; set; }
}