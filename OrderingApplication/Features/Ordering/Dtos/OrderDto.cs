using OrderingDomain.Orders;
using OrderingDomain.ValueTypes;

namespace OrderingApplication.Features.Ordering.Dtos;

internal readonly record struct OrderDto(
    WorldGridPos ShippingWorldGridPos,
    WorldGridPos BillingWorldGridPos,
    bool SaveNewAddress,
    DateTime OrderDate,
    Pricing Pricing,
    OrderState State,
    List<OrderGroupDto> OrderGroups )
{
    internal static Order Model( OrderDto order ) => new() {
        ShippingWorldGridPos = order.ShippingWorldGridPos,
        BillingWorldGridPos = order.BillingWorldGridPos,
        OrderDate = order.OrderDate,
        Pricing = order.Pricing,
        State = OrderState.Processing
    };
    internal static Order Model( OrderDto order, Guid customerId ) => new() {
        CustomerId = customerId,
        ShippingWorldGridPos = order.ShippingWorldGridPos,
        BillingWorldGridPos = order.BillingWorldGridPos,
        OrderDate = order.OrderDate,
        Pricing = order.Pricing,
        State = OrderState.Processing
    };
};