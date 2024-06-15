namespace OrderingDomain.Orders;

public sealed class OrderProblem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid OrderLineId { get; set; }
}