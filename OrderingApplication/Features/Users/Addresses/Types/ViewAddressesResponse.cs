namespace OrderingApplication.Features.Users.Addresses.Types;

internal readonly record struct ViewAddressesResponse(
    int TotalCount,
    IEnumerable<AddressDto> Addresses );