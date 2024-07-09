using OrderingDomain.Orders.Base;
using OrderingDomain.Orders.Meta;

namespace OrderingApplication.Features.Users.Orders;

internal readonly record struct AccountOrdersViewDto(
    Guid OrderId,
    DateTime OrderDate,
    OrderState State,
    int TotalQuantity,
    decimal TotalPrice )
{
    internal static AccountOrdersViewDto FromModel( Order order ) =>
        new( order.Id, order.DatePlaced, order.State, order.TotalQuantity, order.TotalPrice );
}