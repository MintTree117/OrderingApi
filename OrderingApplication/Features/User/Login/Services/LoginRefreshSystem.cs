using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.User.Utilities;
using OrderingDomain.Account;
using OrderingDomain.ReplyTypes;

namespace OrderingApplication.Features.User.Login.Services;

internal sealed class LoginRefreshSystem( AccountConfig config, UserManager<UserAccount> userManager, ILogger<LoginRefreshSystem> logger )
{
    readonly JwtConfig _jwtConfig = config.JwtConfigRules;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly ILogger<LoginRefreshSystem> _logger = logger;

    internal async Task<Reply<string>> GetAccessToken( string userId )
    {
        Reply<UserAccount> userReply = await _userManager.FindById( userId );
        if (!userReply.Succeeded)
        {
            _logger.LogInformation( $"User id not found in during access token refresh. {userId}" );
            return Reply<string>.Failure( userReply );
        }
        
        string token = JwtUtils.GenerateAccessToken( userReply.Data, _jwtConfig );
        _logger.LogInformation( $"Access token refreshed. {userId}" );
        return Reply<string>.Success( token );
    }
}