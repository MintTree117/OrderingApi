using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Account.Login.Types;
using OrderingApplication.Features.Account.Utilities;
using OrderingDomain.Account;
using OrderingDomain.Optionals;

namespace OrderingApplication.Features.Account.Login.Services;

internal sealed class LoginSystem( IdentityConfigCache configCache, UserManager<UserAccount> userManager, Login2FaSystem faSystem )
{
    readonly JwtConfig _jwtConfig = configCache.JwtConfigRules;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly Login2FaSystem _2FaSystem = faSystem;
    readonly bool _requiresConfirmedEmail = userManager.Options.SignIn.RequireConfirmedEmail;
    
    // LOGIN
    internal async Task<Reply<LoginResponse>> Login( LoginRequest request )
    {        
        Reply<UserAccount> userReply = await ValidateLogin( request );
        return !userReply.IsSuccess
            ? await HandleLoginFail( userReply )
            : await Utils.Is2FaRequired( _userManager, userReply.Data )
                ? await _2FaSystem.HandleRequires2Fa( userReply )
                : HandleNormalLogin( userReply );
    }
    async Task<Reply<UserAccount>> ValidateLogin( LoginRequest login )
    {
        Reply<bool> validationResult = IReply.Okay();
        
        bool validated =
            (await _userManager.FindByEmailOrUsername( login.EmailOrUsername )).Succeeds( out Reply<UserAccount> userResult ) &&
            (await Utils.IsAccountValid( _userManager, userResult, _requiresConfirmedEmail )).Succeeds( out validationResult ) &&
            (await IsPasswordValid( userResult, login )).Succeeds( out validationResult ) &&
            (await ClearAccessFailCount( userResult )).Succeeds( out validationResult );

        return validated
            ? userResult
            : Reply<UserAccount>.None( $"{userResult.Message()} : {validationResult.Message()}" );
    }
    async Task<Reply<bool>> IsPasswordValid( Reply<UserAccount> u, LoginRequest login )
    {
        bool success = await _userManager.CheckPasswordAsync( u.Data, login.Password );
        return success
            ? IReply.Okay()
            : IReply.None( "Invalid password." );
    }
    async Task<Reply<bool>> ClearAccessFailCount( Reply<UserAccount> user )
    {
        IdentityResult result = await _userManager.ResetAccessFailedCountAsync( user.Data );
        return result.Succeeded
            ? IReply.Okay()
            : IReply.None( $"Failed to reset access count: {result.CombineErrors()}" );
    }
    async Task<Reply<LoginResponse>> HandleLoginFail( Reply<UserAccount> user )
    {
        IReply processReply = await Utils.ProcessAccessFailure( _userManager, user.Data, user.Message() );
        return Reply<LoginResponse>.None( processReply );
    }
    Reply<LoginResponse> HandleNormalLogin( Reply<UserAccount> user )
    {
        string token = JwtUtils.GenerateAccessToken( user.Data, _jwtConfig );
        return Reply<LoginResponse>.With( LoginResponse.LoggedIn( token ) );
    }
}
