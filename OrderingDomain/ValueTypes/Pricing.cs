namespace OrderingDomain.ValueTypes;

public readonly record struct Pricing(
    decimal Subtotal,
    decimal ItemDiscount,
    decimal Tax,
    decimal ShippingCharges,
    decimal TotalPrice, 
    decimal TotalDiscount,
    decimal GrandTotal );