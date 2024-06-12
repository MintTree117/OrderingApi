using OrderingDomain.Account;
using OrderingDomain.ValueTypes;

namespace OrderingApplication.Features.Account.Addresses.Types;

internal readonly record struct AddressDto(
    Guid? Id,
    bool IsPrimary,
    int GridX,
    int GridY )
{
    internal static IEnumerable<AddressDto> FromModels( IEnumerable<UserAddress> models )
    {
        List<AddressDto> dtos = [];
        dtos.AddRange( from m in models select FromModel( m ) );
        return dtos;
    }
    internal static AddressDto FromModel( UserAddress model ) =>
        new( model.Id, model.IsPrimary, model.Address.GridX, model.Address.GridY );
    internal UserAddress ToModel( string identityId ) =>
        new( Id ?? Guid.Empty, identityId, new Address( GridX, GridY ), IsPrimary );
}