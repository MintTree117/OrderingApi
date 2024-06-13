using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Users.Utilities;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingInfrastructure.Features.Account.Repositories;

namespace OrderingApplication.Features.Users.Authentication.Services;

internal sealed class SessionManager( UserConfigCache configCache, UserManager<UserAccount> userManager, ISessionRepository sessions )
{
    readonly JwtConfig _jwtConfig = configCache.JwtConfigRules;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly ISessionRepository _sessions = sessions;
    
    internal async Task<Reply<string>> GetRefreshedToken( string sessionId, string userId )
    {
        var session = await _sessions.GetSession( sessionId, userId );
        if (!session)
            return Reply<string>.NotFound( "Session doesn't exist." );

        var userReply = await _userManager.FindById( userId );
        if (!userReply)
            return Reply<string>.UserNotFound( userReply );
        
        session.Data.LastActive = DateTime.Now;
        await _sessions.SaveAsync();
        
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

        var sessionReply = await _sessions.AddSession( session );
        return sessionReply;
    }
    internal async Task<IReply> RevokeSession( string sessionId, string userId )
    {
        var sessionReply = await _sessions.DeleteSession( sessionId, userId );
        return sessionReply;
    }
}