using OrderingApplication.Features.Identity.AccountAddresses.Types;
using OrderingDomain.Identity;
using OrderingDomain.Optionals;
using OrderingInfrastructure;
using OrderingInfrastructure.Features.Identity.Repositories;

namespace OrderingApplication.Features.Identity.AccountAddresses;

internal sealed class UserAddressManager( IIdentityAddressRepository addressRepository )
{
    readonly IIdentityAddressRepository _repo = addressRepository;

    internal async Task<Reply<bool>> AddAddress( string userId, AddressDto request )
    {
        UserAddress model = request.ToModel( userId );

        Replies<UserAddress> any = await _repo.GetAllAddresses( userId );
        if (!any.IsSuccess)
            return IReply.None( any );

        if (!any.Enumerable.Any())
            model.IsPrimary = true;
        
        return await _repo.AddAddress( model );
    }
    internal async Task<Reply<bool>> UpdateAddress( string userId, AddressDto request )
    {
        UserAddress model = request.ToModel( userId );

        // Ensure there can only be one primary address at a time
        Replies<UserAddress> otherPrimaryAddresses = await _repo.GetAllAddresses( userId );
        if (model.IsPrimary) {
            foreach ( UserAddress otherAddress in otherPrimaryAddresses.Enumerable ) {
                // Set all other addresses to not be primary
                otherAddress.IsPrimary = false;
                await _repo.UpdateAddress( otherAddress );
            }
        }
        else if (!otherPrimaryAddresses.Enumerable.Any( static a => a.IsPrimary ))
                return IReply.None( "There must be at least one primary address." );
        
        return await _repo.UpdateAddress( model );
    }
    internal async Task<Reply<bool>> DeleteAddress( string userId, Guid addressId )
    {
        Reply<UserAddress> address = await _repo.GetAddress( addressId );

        if (!address.IsSuccess)
            return IReply.None( address );
        
        bool wasPrimary = address.Data.IsPrimary;
        Reply<bool> deleteResult = await _repo.DeleteAddress( address.Data );

        if (!deleteResult.IsSuccess)
            return IReply.None( deleteResult );

        if (!wasPrimary)
            return IReply.Okay();

        Replies<UserAddress> addresses = await _repo.GetAllAddresses( userId );
        if (!addresses.Enumerable.Any())
            return IReply.Okay();

        UserAddress newPrimary = addresses.Enumerable.First();
        newPrimary.IsPrimary = true;

        await _repo.UpdateAddress( newPrimary );
        return IReply.Okay(); // Its still success if deleted but not updated
    }
    internal async Task<Reply<ViewAddressesResponse>> ViewAddresses( string userId, int page, int pageSize )
    {
        Reply<PagedResult<UserAddress>> result = await _repo.GetPagedAddresses( userId, page, pageSize );
        IEnumerable<AddressDto> dtos = AddressDto.FromModels( result.Data.Items );
        return result.IsSuccess
            ? Reply<ViewAddressesResponse>.With( ViewAddressesResponse.With( result.Data.TotalCount, dtos ) )
            : Reply<ViewAddressesResponse>.None( result );
    }
}