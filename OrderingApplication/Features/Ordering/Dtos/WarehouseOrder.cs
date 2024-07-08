using OrderingApplication.Features.Cart;

namespace OrderingApplication.Features.Ordering.Dtos;

internal readonly record struct WarehouseOrder(
    Guid OrderId,
    Guid OrderGroupId,
    string? CustomerId,
    DateTime DateCreated,
    List<CartItemDto> Items );