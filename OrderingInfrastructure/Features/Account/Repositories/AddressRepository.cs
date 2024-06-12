using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderingDomain.Account;
using OrderingDomain.ReplyTypes;

namespace OrderingInfrastructure.Features.Account.Repositories;

internal sealed class AddressRepository( AccountDbContext database, ILogger<AddressRepository> logger ) 
    : DatabaseService<AddressRepository>( database, logger ), IAddressRepository
{
    readonly AccountDbContext _database = database;
    
    public async Task<Reply<UserAddress>> GetAddress( Guid addressId )
    {
        try {
            UserAddress? result = await _database.Addresses.FirstOrDefaultAsync( a => a.Id == addressId );
            return result is not null
                ? Reply<UserAddress>.Success( result )
                : Reply<UserAddress>.Failure( $"No address found with id {addressId}." );
        }
        catch ( Exception e ) {
            return HandleDbException<UserAddress>( e );
        }
    }
    public async Task<Replies<UserAddress>> GetAllAddresses( string userId )
    {
        try {
            List<UserAddress> result =
                await _database.Addresses
                        .Where( a => a.UserId == userId )
                        .ToListAsync();
            return Replies<UserAddress>.Success( result );
        }
        catch ( Exception e ) {
            return HandleDbExceptionOpts<UserAddress>( e );
        }
    }
    public async Task<Reply<PagedResult<UserAddress>>> GetPagedAddresses( string userId, int page, int results )
    {
        try {
            int totalCount = await _database.Addresses.CountAsync( a => a.UserId == userId );
            List<UserAddress> result =
                await _database.Addresses
                        .Where( a => a.UserId == userId )
                        .Skip( results * GetPage( page ) )
                        .Take( results )
                        .ToListAsync();
            return Reply<PagedResult<UserAddress>>
                .Success( PagedResult<UserAddress>
                    .With( totalCount, result ) );
        }
        catch ( Exception e ) {
            return HandleDbException<PagedResult<UserAddress>>( e );
        }

        static int GetPage( int page ) =>
            Math.Max( 0, page - 1 );
    }
    public async Task<Reply<bool>> AddAddress( UserAddress address )
    {
        try {
            await _database.Addresses.AddAsync( address );
            return await SaveAsync();
        }
        catch ( Exception e ) {
            return HandleDbException<bool>( e );
        }
    }
    public async Task<Reply<bool>> UpdateAddress( UserAddress address )
    {
        try {
            UserAddress? model = await _database.Addresses.FirstOrDefaultAsync( a => a.Id == address.Id );

            if (model is null)
                return IReply.Fail( "Address id not found." );

            model.Address = address.Address;
            model.IsPrimary = address.IsPrimary;
            
            _database.Addresses.Update( model );
            return await SaveAsync();
        }
        catch ( Exception e ) {
            return HandleDbException<bool>( e );
        }
    }
    public async Task<Reply<bool>> DeleteAddress( UserAddress address )
    {
        try {
            _database.Addresses.Remove( address );
            return await SaveAsync();
        }
        catch ( Exception e ) {
            return HandleDbException<bool>( e );
        }
    }
}