using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Users.Authentication.Types;
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

    internal async Task<Reply<ViewUserSessionsDto>> ViewUserSessions( string userId, int page, int pageSize )
    {
        var userReply = await _userManager.FindById( userId );
        if (!userReply)
        {
            Logger.LogError( "User not found!" );
            return Reply<ViewUserSessionsDto>.UserNotFound();            
        }

        using var countTask = _sessions.CountUserSessions( userId );
        using var paginateTask = _sessions.GetPaginatedUserSessions( userId, page, pageSize );
        
        await Task.WhenAll( countTask, paginateTask );

        if (!countTask.Result)
        {
            Logger.LogError( "Failed count task" + countTask.Result.GetMessage() );
            return Reply<ViewUserSessionsDto>.ServerError( countTask.Result.GetMessage() );
        }
        if (!paginateTask.Result)
        {
            Logger.LogError( "Failed paginate task." + paginateTask.Result.GetMessage() );
            return Reply<ViewUserSessionsDto>.ServerError( paginateTask.Result.GetMessage() );
        }

        var dto = ViewUserSessionsDto.FromModels( countTask.Result.Data, paginateTask.Result.Data );
        return Reply<ViewUserSessionsDto>.Success( dto );
    }
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
    internal async Task<Reply<bool>> AddSession( string sessionId, string userId )
    {
        UserSession session = new() {
            Id = sessionId,
            UserId = userId,
            DateCreated = DateTime.Now,
            LastActive = DateTime.Now
        };
        IReply addReply = await _sessions.AddSession( session );
        LogIfErrorReply( addReply );
        return IReply.Success();
    }
    internal async Task<Reply<bool>> RevokeSession( string sessionId, string userId )
    {
        IReply deleteReply = await _sessions.DeleteSession( sessionId, userId );
        LogIfErrorReply( deleteReply );
        return deleteReply.CheckSuccess()
            ? IReply.Success()
            : IReply.Fail( deleteReply );
    }
}