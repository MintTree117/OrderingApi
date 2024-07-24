using OrderingDomain.Orders;

namespace OrderingApplication.Features.Ordering.Dtos;

internal readonly record struct OrderUpdateRequest(
    Guid OrderId,
    Guid OrderGroupId,
    OrderStatus OrderStatus );