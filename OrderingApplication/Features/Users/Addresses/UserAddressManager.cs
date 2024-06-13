using OrderingApplication.Features.Users.Addresses.Types;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingDomain.ValueTypes;
using OrderingInfrastructure.Features.Users.Repositories;

namespace OrderingApplication.Features.Users.Addresses;

internal sealed class UserAddressManager( IAddressRepository addressRepository )
{
    readonly IAddressRepository _repo = addressRepository;

    internal async Task<IReply> AddAddress( string userId, Address address )
    {
        UserAddress model = new( Guid.Empty, userId, false, address );
        
        var addresses = await _repo.GetAllAddresses( userId );
        
        if (!addresses)
            return IReply.ServerError( addresses );
        
        if (!addresses.Enumerable.Any())
            model.IsPrimary = true;
        
        return await _repo.AddAddress( model );
    }
    internal async Task<IReply> UpdateAddress( string userId, AddressDto request )
    {
        var model = request.ToModel( userId );
        var otherAddresses = await _repo.GetAllAddresses( userId ); // Ensure there can only be one primary address at a time

        if (!otherAddresses)
            return IReply.NotFound( otherAddresses );

        var alreadyPrimary = otherAddresses.Enumerable.Any( static a => a.IsPrimary );

        switch ( model.IsPrimary )
        {
            case false when !alreadyPrimary:
                return IReply.Conflict( "There must be a primary address." );
            case true when alreadyPrimary:
                return IReply.Conflict( "There can only be one primary address." );
            case false:
                return await _repo.UpdateAddress( model );
        }

        foreach ( UserAddress otherAddress in otherAddresses.Enumerable ) { // Set all other addresses to not be primary
            otherAddress.IsPrimary = false;
            await _repo.UpdateAddress( otherAddress );
        }

        return await _repo.UpdateAddress( model );
    }
    internal async Task<IReply> DeleteAddress( string userId, Guid addressId )
    {
        var address = await _repo.GetAddress( addressId );
        if (!address)
            return IReply.NotFound( address );
        
        var wasPrimary = address.Data.IsPrimary;
        var deleteResult = await _repo.DeleteAddress( address.Data );

        if (!deleteResult.CheckSuccess())
            return IReply.ServerError( deleteResult );

        if (!wasPrimary)
            return IReply.Success();

        var addresses = await _repo.GetAllAddresses( userId );
        if (addresses && !addresses.Enumerable.Any())
            return IReply.Success();

        var newPrimary = addresses.Enumerable.First();
        newPrimary.IsPrimary = true;

        await _repo.UpdateAddress( newPrimary );
        return IReply.Success(); // Its still success if deleted but not updated
    }
    internal async Task<Reply<ViewAddressesResponse>> ViewAddresses( string userId, int page, int pageSize )
    {
        var getReply = await _repo.GetPagedAddresses( userId, page, pageSize );
        var dtos = AddressDto.FromModels( getReply.Data.Items );
        return getReply.Succeeded
            ? Reply<ViewAddressesResponse>.Success( new ViewAddressesResponse( getReply.Data.TotalCount, dtos ) )
            : Reply<ViewAddressesResponse>.ServerError( getReply );
    }
}