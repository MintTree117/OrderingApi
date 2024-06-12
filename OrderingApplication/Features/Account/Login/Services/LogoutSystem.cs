using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Account.Utilities;
using OrderingDomain.Account;
using OrderingDomain.Optionals;

namespace OrderingApplication.Features.Account.Login.Services;

internal sealed class LogoutSystem( IdentityConfigCache configCache, UserManager<UserAccount> userManager, Login2FaSystem faSystem )
{
    readonly JwtConfig _jwtConfig = configCache.JwtConfigRules;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly Login2FaSystem _2FaSystem = faSystem;
    readonly bool _requiresConfirmedEmail = userManager.Options.SignIn.RequireConfirmedEmail;
    
    internal async Task<Reply<bool>> Logout( string userId, ISession session )
    {
        bool success = false;
        return success
            ? IReply.Okay()
            : IReply.None( "Invalid password." );
    }
}