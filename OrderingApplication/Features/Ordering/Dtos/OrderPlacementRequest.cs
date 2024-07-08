using OrderingApplication.Features.Cart;
using OrderingDomain.ValueTypes;

namespace OrderingApplication.Features.Ordering.Dtos;

internal readonly record struct OrderPlacementRequest(
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    Address BillingAddress,
    Address ShippingAddress,
    List<CartItemDto> Items )
{
    internal Contact GetContact() =>
        new Contact( CustomerName, CustomerEmail, CustomerPhone );
}