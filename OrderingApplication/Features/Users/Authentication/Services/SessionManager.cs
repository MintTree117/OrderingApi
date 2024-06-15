using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Users.Utilities;
using OrderingApplication.Utilities;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingInfrastructure.Features.Users.Repositories;

namespace OrderingApplication.Features.Users.Authentication.Services;

internal sealed class SessionManager( UserManager<UserAccount> userManager, ISessionRepository sessions, ILogger<SessionManager> logger )
    : BaseService<SessionManager>( logger )
{
    readonly JwtConfig _jwtConfig = UserConsts.Instance.JwtConfigRules;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly ISessionRepository _sessions = sessions;
    
    internal async Task<Reply<string>> UpdateSession( string sessionId, string userId )
    {
        var session = await _sessions.GetSession( sessionId, userId );
        LogIfErrorReply( session );
        if (!session)
            return Reply<string>.NotFound( "Session doesn't exist." );

        var userReply = await _userManager.FindById( userId );
        if (!userReply)
            return Reply<string>.UserNotFound();
        
        session.Data.LastActive = DateTime.Now;
        var saveReply = await _sessions.SaveAsync();
        LogIfErrorReply( saveReply );
        
        JwtUtils.GenerateAccessToken( userReply.Data, _jwtConfig, out string accessToken );
        return Reply<string>.Success( accessToken );
    }
    internal async Task<IReply> AddSession( string sessionId, string userId )
    {
        UserSession session = new() {
            Id = sessionId,
            UserId = userId,
            DateCreated = DateTime.Now,
            LastActive = DateTime.Now
        };
        var addReply = await _sessions.AddSession( session );
        LogIfErrorReply( addReply );
        return addReply;
    }
    internal async Task<IReply> RevokeSession( string sessionId, string userId )
    {
        var deleteReply = await _sessions.DeleteSession( sessionId, userId );
        LogIfErrorReply( deleteReply );
        return deleteReply;
    }
}