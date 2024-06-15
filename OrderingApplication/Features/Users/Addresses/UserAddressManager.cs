using OrderingApplication.Features.Users.Addresses.Types;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingDomain.ValueTypes;
using OrderingInfrastructure.Features.Users.Repositories;

namespace OrderingApplication.Features.Users.Addresses;

internal sealed class UserAddressManager( IAddressRepository addressRepository, ILogger<UserAddressManager> logger )
    : BaseService<UserAddressManager>( logger )
{
    readonly IAddressRepository _repository = addressRepository;

    internal async Task<IReply> AddAddress( string userId, Address address )
    {
        var model = new UserAddress( Guid.Empty, userId, false, address );
        var getReply = await _repository.GetAllAddresses( userId );
        LogReplyError( getReply );
        if (!getReply)
            return IReply.ServerError( getReply );
        
        if (!getReply.Enumerable.Any())
            model.IsPrimary = true;
        
        var addReply = await _repository.AddAddress( model );
        LogReplyError( addReply );
        return addReply;
    }
    internal async Task<IReply> UpdateAddress( string userId, AddressDto request )
    {
        var model = request.ToModel( userId );
        var getReply = await _repository.GetAllAddresses( userId ); // Ensure there can only be one primary address at a time
        LogReplyError( getReply );
        if (!getReply)
            return IReply.NotFound( getReply );

        var alreadyPrimary = getReply.Enumerable.Any( static a => a.IsPrimary );
        if (!request.IsPrimary && !alreadyPrimary)
            return IReply.Conflict( "There must be a primary address." );

        foreach ( UserAddress otherAddress in getReply.Enumerable ) { // Set all other addresses to not be primary
            otherAddress.IsPrimary = false;
            var updateReply = await _repository.UpdateAddress( otherAddress );
            LogReplyError( updateReply );
        }

        var updated = await _repository.UpdateAddress( model );
        LogReplyError( updated );
        return updated;
    }
    internal async Task<IReply> DeleteAddress( string userId, Guid addressId )
    {
        var getReply = await _repository.GetAddress( addressId );
        LogReplyError( getReply );
        if (!getReply)
            return IReply.NotFound( getReply );
        
        var wasPrimary = getReply.Data.IsPrimary;
        var deleteReply = await _repository.DeleteAddress( getReply.Data );
        LogReplyError( deleteReply );

        if (!deleteReply.CheckSuccess())
            return IReply.ServerError( deleteReply );
        if (!wasPrimary)
            return IReply.Success();

        var getAllReply = await _repository.GetAllAddresses( userId );
        LogReplyError( getAllReply );
        if (getAllReply && !getAllReply.Enumerable.Any())
            return IReply.Success();

        var newPrimary = getAllReply.Enumerable.First();
        newPrimary.IsPrimary = true;

        var saveReply = await _repository.SaveAsync();
        LogReplyError( saveReply );
        return IReply.Success(); // Its still success if deleted but not updated
    }
    internal async Task<Reply<ViewAddressesResponse>> ViewAddresses( string userId, int page, int pageSize )
    {
        var getReply = await _repository.GetPagedAddresses( userId, page, pageSize );
        LogReplyError( getReply );
        if (!getReply)
            return Reply<ViewAddressesResponse>.NotFound();
        var dtos = AddressDto.FromModels( getReply.Data.Items );
        return Reply<ViewAddressesResponse>.Success( new ViewAddressesResponse( getReply.Data.TotalCount, dtos ) );
    }
}