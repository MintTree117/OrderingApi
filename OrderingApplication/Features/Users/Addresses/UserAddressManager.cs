using OrderingApplication.Features.Users.Addresses.Types;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingInfrastructure.Features.Users.Repositories;

namespace OrderingApplication.Features.Users.Addresses;

internal sealed class UserAddressManager( IAddressRepository addressRepository, ILogger<UserAddressManager> logger )
    : BaseService<UserAddressManager>( logger )
{
    readonly IAddressRepository _repository = addressRepository;

    internal async Task<Reply<bool>> AddAddress( string userId, AddressDto address )
    {
        // fetch existing
        var getReply = await _repository.GetAllByUserId( userId );
        LogIfErrorReply( getReply );
        if (!getReply)
            return IReply.Fail( getReply );

        var model = address.ToModel( userId );
        
        // ensure only one primary address
        if (model.IsPrimary)
            foreach ( UserAddress otherAddress in getReply.Enumerable )
                otherAddress.IsPrimary = false;
        var saveReply = await _repository.SaveAsync();
        LogIfErrorReply( saveReply );
        if (!saveReply)
            return IReply.ServerError();
        
        // ensure always a primary address
        if (!getReply.Enumerable.Any())
            model.IsPrimary = true;
        
        // add
        var addReply = await _repository.Add( model );
        LogIfErrorReply( addReply );
        return addReply;
    }
    internal async Task<Reply<bool>> UpdateAddress( string userId, AddressDto address )
    {
        // fetch existing
        var getReply = await _repository.GetAllByUserId( userId );
        LogIfErrorReply( getReply );
        if (!getReply)
            return IReply.Fail( getReply );
        
        // validate address
        UserAddress? modelToUpdate = getReply.Enumerable.FirstOrDefault( m => m.Id == address.Id );
        if (modelToUpdate is null)
            return IReply.NotFound( "Address not found." );

        // ensure always a primary address
        var alreadyPrimary = getReply.Enumerable.FirstOrDefault( static a => a.IsPrimary );
        if (!address.IsPrimary && alreadyPrimary is null )
            return IReply.Conflict( "There must be a primary address." );

        // ensure only one primary address
        if (address.IsPrimary)
            foreach ( UserAddress otherAddress in getReply.Enumerable )
                otherAddress.IsPrimary = false;
        
        // update model
        modelToUpdate.IsPrimary = address.IsPrimary;
        modelToUpdate.Name = address.Name;
        modelToUpdate.WorldGridPosX = address.WorldGridPosX;
        modelToUpdate.WorldGridPosY = address.WorldGridPosY;
        
        // save
        var saveReply = await _repository.SaveAsync();
        LogIfErrorReply( saveReply );
        return saveReply;
    }
    internal async Task<Reply<bool>> DeleteAddress( string userId, Guid addressId )
    {
        // get model to delete
        var getReply = await _repository.GetById( addressId );
        LogIfErrorReply( getReply );
        if (!getReply)
            return IReply.NotFound( $"Address not found with id {addressId}." );
        
        // delete the model
        var wasPrimary = getReply.Data.IsPrimary;
        var deleteReply = await _repository.Delete( getReply.Data );
        LogIfErrorReply( deleteReply );
        if (!deleteReply.CheckSuccess())
            return IReply.ServerError( deleteReply );
        if (!wasPrimary)
            return IReply.Success();

        // ensure always a primary address
        var getAllReply = await _repository.GetAllByUserId( userId );
        LogIfErrorReply( getAllReply );
        if (!getAllReply || !getAllReply.Enumerable.Any())
            return IReply.Success(); // its still success if addresses cause error; the error is logged right above if it exists

        var newPrimary = getAllReply.Enumerable.First();
        newPrimary.IsPrimary = true;

        var saveReply = await _repository.SaveAsync();
        LogIfErrorReply( saveReply );
        return IReply.Success(); // its still success if deleted but not updated; the error is logged right above if it exists
    }
    internal async Task<Reply<ViewAddressesResponse>> ViewAddresses( string userId, int page, int pageSize )
    {
        var getReply = await _repository.GetAllByUserIdPaged( userId, page, pageSize );
        LogIfErrorReply( getReply );
        if (!getReply)
            return Reply<ViewAddressesResponse>.NotFound();
        var dtos = AddressDto.FromModels( getReply.Data.Items );
        var response = new ViewAddressesResponse( getReply.Data.TotalCount, dtos );
        return Reply<ViewAddressesResponse>.Success( response );
    }
}