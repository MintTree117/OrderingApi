namespace OrderingDomain.ValueTypes;

public sealed class Pricing
{
    public Guid Id { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TotalShipping { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalPrice { get; set; }
}