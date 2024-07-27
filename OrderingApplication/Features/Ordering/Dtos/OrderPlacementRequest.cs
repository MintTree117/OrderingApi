using OrderingApplication.Features.Cart;
using OrderingDomain.ValueTypes;

namespace OrderingApplication.Features.Ordering.Dtos;

internal readonly record struct OrderPlacementRequest(
    Contact Contact,
    Address BillingAddress,
    Address ShippingAddress,
    List<CartItemDto> Items );