using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.User.Login.Types;
using OrderingApplication.Features.User.Utilities;
using OrderingDomain.Account;
using OrderingDomain.ReplyTypes;

namespace OrderingApplication.Features.User.Login.Services;

internal sealed class LoginSystem( AccountConfig config, UserManager<UserAccount> userManager, Login2FaSystem faSystem )
{
    readonly JwtConfig _jwtConfig = config.JwtConfigRules;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly Login2FaSystem _2FaSystem = faSystem;
    readonly bool _requiresConfirmedEmail = userManager.Options.SignIn.RequireConfirmedEmail;
    
    // LOGIN
    internal async Task<Reply<LoginResponse>> Login( LoginRequest request )
    {        
        var user = await ValidateLogin( request );
        if (!user)
        {
            
        }
        
        return !user
            ? await HandleLoginFail( user )
            : await _userManager.Is2FaRequired( user.Data )
                ? await _2FaSystem.HandleRequires2Fa( user )
                : HandleNormalLogin( user );
    }
    async Task<Reply<UserAccount>> ValidateLogin( LoginRequest login )
    {
        Reply<bool> validationResult = IReply.Success();

        bool validated =
            (await _userManager.FindByEmailOrUsername( login.EmailOrUsername )).OutSuccess( out Reply<UserAccount> userResult ) &&
            (await _userManager.IsAccountValid( userResult, _requiresConfirmedEmail )).OutSuccess( out validationResult ) &&
            (await _userManager.CheckPasswordAsync( userResult.Data, login.Password ) &&
            (await ClearAccessFailCount( userResult )).OutSuccess( out validationResult ));

        return validated
            ? userResult
            : Reply<UserAccount>.Failure( $"{userResult.GetMessage()} : {validationResult.GetMessage()}" );
    }
    async Task<Reply<bool>> ClearAccessFailCount( Reply<UserAccount> user )
    {
        IdentityResult result = await _userManager.ResetAccessFailedCountAsync( user.Data );
        return result.Succeeded
            ? IReply.Success()
            : IReply.Fail( $"Failed to reset access count: {result.CombineErrors()}" );
    }
    async Task<Reply<LoginResponse>> HandleLoginFail( Reply<UserAccount> user )
    {
        IReply processReply = await _userManager.ProcessAccessFailure( user.Data );
        return Reply<LoginResponse>.Failure( processReply );
    }
    Reply<LoginResponse> HandleNormalLogin( Reply<UserAccount> user )
    {
        string token = JwtUtils.GenerateAccessToken( user.Data, _jwtConfig );
        return Reply<LoginResponse>.Success( LoginResponse.LoggedIn( token ) );
    }
}
