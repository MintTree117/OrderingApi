using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.User.Utilities;
using OrderingDomain.Account;
using OrderingDomain.ReplyTypes;

namespace OrderingApplication.Features.User.Login.Services;

internal sealed class LogoutSystem( AccountConfig config, UserManager<UserAccount> userManager, Login2FaSystem faSystem )
{
    readonly JwtConfig _jwtConfig = config.JwtConfigRules;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly Login2FaSystem _2FaSystem = faSystem;
    readonly bool _requiresConfirmedEmail = userManager.Options.SignIn.RequireConfirmedEmail;
    
    internal async Task<Reply<bool>> Logout( string userId, ISession session )
    {
        await Task.Delay( 1000 );
        bool success = false;
        return success
            ? IReply.Success()
            : IReply.Fail( "Invalid password." );
    }
}