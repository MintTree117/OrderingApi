namespace OrderingApplication.Features.User.Addresses.Types;

internal readonly record struct ViewAddressesResponse(
    int TotalCount,
    IEnumerable<AddressDto> Addresses );