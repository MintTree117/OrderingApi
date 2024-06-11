using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Identity.Utilities;
using OrderingDomain.Identity;
using OrderingDomain.Optionals;

namespace OrderingApplication.Features.Identity.Login.Services;

internal sealed class LoginJwtRefresher( IdentityConfigCache configCache, UserManager<UserAccount> userManager, ILogger<LoginJwtRefresher> logger )
{
    readonly JwtConfig _jwtConfig = configCache.JwtConfigRules;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly ILogger<LoginJwtRefresher> _logger = logger;

    internal async Task<Reply<string>> GetAccessToken( string userId )
    {
        Reply<UserAccount> userReply = await _userManager.FindById( userId );
        if (!userReply.IsSuccess)
            return Reply<string>.None( userReply );

        string token = IdentityTokenUtils.GenerateAccessToken( userId, _jwtConfig );
        return Reply<string>.With( token );
    }
}