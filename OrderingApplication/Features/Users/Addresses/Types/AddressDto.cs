using OrderingDomain.Users;

namespace OrderingApplication.Features.Users.Addresses.Types;

internal readonly record struct AddressDto(
    Guid Id,
    bool IsPrimary,
    string Name,
    int WorldGridPosX,
    int WorldGridPosY )
{
    internal static IEnumerable<AddressDto> FromModels( IEnumerable<UserAddress> models )
    {
        List<AddressDto> dtos = [];
        dtos.AddRange( from m in models select FromModel( m ) );
        return dtos;
    }
    internal static AddressDto FromModel( UserAddress model ) =>
        new( model.Id, model.IsPrimary, model.Name, model.WorldGridPosX, model.WorldGridPosY );
    internal UserAddress ToModel( string identityId ) =>
        new( Id, identityId, IsPrimary, Name, WorldGridPosX, WorldGridPosY );
}