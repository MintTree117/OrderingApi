using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;

namespace OrderingInfrastructure.Features.Users.Repositories;

internal sealed class SessionRepository( UserDbContext database, ILogger<SessionRepository> logger ) 
    : DatabaseService<SessionRepository>( database, logger ), ISessionRepository
{
    readonly UserDbContext _database = database;

    public async Task<Reply<int>> CountUserSessions( string userId )
    {
        try
        {
            int count = await _database.Sessions.CountAsync( s => s.UserId == userId );
            return Reply<int>.Success( count );
        }
        catch ( Exception e )
        {
            logger.LogError( e, e.Message );
            return ProcessDbException<int>( e );
        }
    }
    public async Task<Reply<List<UserSession>>> GetPaginatedUserSessions( string userId, int page, int pageSize )
    {
        try
        {
            int offset = Math.Max( 0, page ) * pageSize;
            List<UserSession> result =
                await _database.Sessions
                    .Where( s => s.UserId == userId )
                    .Skip( offset )
                    .Take( pageSize )
                    .ToListAsync();
            return Reply<List<UserSession>>.Success( result );
        }
        catch ( Exception e )
        {
            logger.LogError( e, e.Message );
            return ProcessDbException<List<UserSession>>( e );
        }
    }
    public async Task<Reply<UserSession>> GetSession( string sessionId, string userId )
    {
        try
        {
            var session = await _database.Sessions.FirstOrDefaultAsync( s => s.Id == sessionId && s.UserId == userId );
            return session is not null
                ? Reply<UserSession>.Success( session )
                : Reply<UserSession>.NotFound();
        }
        catch ( Exception e )
        {
            return ProcessDbException<UserSession>( e );
        }
    }
    public async Task<IReply> AddSession( UserSession session )
    {
        try
        {
            var entry = await _database.AddAsync( session );
            if (entry.State != EntityState.Added)
                return IReply.Fail( entry.State.ToString() );
            
            return await SaveAsync();
        }
        catch ( Exception e )
        {
            return ProcessDbException<bool>( e );
        }
    }
    public async Task<IReply> DeleteSession( string sessionId, string userId )
    {
        try
        {
            UserSession? session = await _database.Sessions.FirstOrDefaultAsync( s => s.Id == sessionId && s.UserId == userId );
            if (session is null)
                return Reply<bool>.Failure( "Session not found." );
            
            
            _database.Sessions.Remove( session );
            return await SaveAsync();
        }
        catch ( Exception e )
        {
            return ProcessDbException<bool>( e );
        }
    }
}