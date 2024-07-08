namespace OrderingDomain.Orders.Meta;

public sealed class OrderProblem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? OrderGroupId { get; set; }
    public string Message { get; set; } = string.Empty;
}