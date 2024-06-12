namespace OrderingApplication.Features.Account.Addresses.Types;

internal readonly record struct ViewAddressesResponse(
    int TotalCount,
    IEnumerable<AddressDto> Addresses )
{
    internal static ViewAddressesResponse With( int count, IEnumerable<AddressDto> addresses ) =>
        new( count, addresses );
}