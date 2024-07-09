using OrderingDomain.Orders;

namespace OrderingApplication.Features.Users.Orders;

public readonly record struct AccountOrderViewDto(
    Guid OrderId,
    DateTime OrderDate,
    OrderState State,
    int TotalQuantity,
    decimal TotalPrice )
{
    internal static AccountOrderViewDto FromModel( Order order ) =>
        new( order.Id, order.DatePlaced, order.State, order.TotalQuantity, order.TotalPrice );
}