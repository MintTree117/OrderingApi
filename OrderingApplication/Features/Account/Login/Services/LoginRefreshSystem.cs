using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Account.Utilities;
using OrderingDomain.Account;
using OrderingDomain.Optionals;

namespace OrderingApplication.Features.Account.Login.Services;

internal sealed class LoginRefreshSystem( IdentityConfigCache configCache, UserManager<UserAccount> userManager, ILogger<LoginRefreshSystem> logger )
{
    readonly JwtConfig _jwtConfig = configCache.JwtConfigRules;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly ILogger<LoginRefreshSystem> _logger = logger;

    internal async Task<Reply<string>> GetAccessToken( string userId )
    {
        Reply<UserAccount> userReply = await _userManager.FindById( userId );
        if (!userReply.IsSuccess)
        {
            _logger.LogInformation( $"User id not found in during access token refresh. {userId}" );
            return Reply<string>.None( userReply );
        }
        
        string token = JwtUtils.GenerateAccessToken( userReply.Data, _jwtConfig );
        _logger.LogInformation( $"Access token refreshed. {userId}" );
        return Reply<string>.With( token );
    }
}