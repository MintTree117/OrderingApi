using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;

namespace OrderingInfrastructure.Features.Users.Repositories;

internal sealed class AddressRepository( UserDbContext database, ILogger<AddressRepository> logger ) 
    : DatabaseService<AddressRepository>( database, logger ), IAddressRepository
{
    readonly UserDbContext _database = database;
    
    public async Task<Reply<UserAddress>> GetById( Guid addressId )
    {
        try {
            UserAddress? result = await _database.Addresses.FirstOrDefaultAsync( a => a.Id == addressId );
            return result is not null
                ? Reply<UserAddress>.Success( result )
                : Reply<UserAddress>.NotFound( $"No address found with id {addressId}." );
        }
        catch ( Exception e ) {
            return ProcessDbException<UserAddress>( e );
        }
    }
    public async Task<Replies<UserAddress>> GetAllByUserId( string userId )
    {
        try {
            List<UserAddress> result =
                await _database.Addresses
                        .Where( a => a.UserId == userId )
                        .ToListAsync();
            return Replies<UserAddress>.Success( result );
        }
        catch ( Exception e ) {
            return HandleDbExceptionReplies<UserAddress>( e );
        }
    }
    public async Task<Reply<PagedResult<UserAddress>>> GetAllByUserIdPaged( string userId, int page, int pageSize )
    {
        try {
            int totalCount = await _database.Addresses.CountAsync( a => a.UserId == userId );
            List<UserAddress> addresses =
                await _database.Addresses
                        .Where( a => a.UserId == userId )
                        .Skip( pageSize * GetPage( page ) )
                        .Take( pageSize )
                        .ToListAsync();
            return Reply<PagedResult<UserAddress>>
                .Success( PagedResult<UserAddress>
                    .With( totalCount, addresses ) );
        }
        catch ( Exception e ) {
            return ProcessDbException<PagedResult<UserAddress>>( e );
        }

        static int GetPage( int page ) =>
            Math.Max( 0, page - 1 );
    }
    public async Task<Reply<bool>> Add( UserAddress address )
    {
        try {
            await _database.Addresses.AddAsync( address );
            return await SaveAsync();
        }
        catch ( Exception e ) {
            return ProcessDbException<bool>( e );
        }
    }
    public async Task<Reply<bool>> Delete( UserAddress address )
    {
        try {
            _database.Addresses.Remove( address );
            return await SaveAsync();
        }
        catch ( Exception e ) {
            return ProcessDbException<bool>( e );
        }
    }
}