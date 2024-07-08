using OrderingDomain.Orders.Meta;

namespace OrderingApplication.Features.Ordering.Dtos;

internal readonly record struct OrderUpdateRequest(
    Guid OrderId,
    Guid OrderGroupId,
    OrderState OrderState );