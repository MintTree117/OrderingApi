namespace OrderingApplication.Features.Ordering.Dtos;

internal readonly record struct OrderCatalogItem(
    Guid ProductId,
    Guid WarehouseId,
    decimal UnitPrice,
    decimal Discount,
    decimal Tax,
    int Quantity );