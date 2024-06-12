using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderingDomain.Account;
using OrderingDomain.Optionals;

namespace OrderingInfrastructure.Features.Account.Repositories;

internal sealed class SessionRepository( AccountDbContext database, ILogger<SessionRepository> logger ) 
    : DatabaseService<SessionRepository>( database, logger ), ISessionRepository
{
    readonly AccountDbContext _database = database;

    public async Task<Replies<UserSession>> GetSessionsByUserId( string userId )
    {
        try
        {
            List<UserSession> result =
                await _database.Sessions
                    .Where( s => s.UserId == userId )
                    .ToListAsync();
            return Replies<UserSession>.With( result );
        }
        catch ( Exception e )
        {
            return HandleDbExceptionOpts<UserSession>( e );
        }
    }
    public async Task<Reply<bool>> DeleteSession( Guid sessionId )
    {
        try
        {
            UserSession? session = await _database.Sessions.FirstOrDefaultAsync( s => s.Id == sessionId );
            if (session is null)
                return Reply<bool>.None( "Session not found." );
            
            
            _database.Sessions.Remove( session );
            return await SaveAsync();
        }
        catch ( Exception e )
        {
            return HandleDbException<bool>( e );
        }
    }

}