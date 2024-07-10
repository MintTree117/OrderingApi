namespace OrderingApplication.Features.Ordering.Dtos;

internal readonly record struct OrderCatalogItem(
    Guid ProductId,
    Guid WarehouseId,
    string UnitName,
    decimal UnitPrice,
    decimal UnitDiscount,
    decimal ShippingCost,
    int Quantity );
