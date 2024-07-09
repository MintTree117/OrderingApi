namespace OrderingApplication.Features.Users.Orders;

public readonly record struct AccountOrdersViewDto(
    int TotalCount,
    List<AccountOrderViewDto> Orders );