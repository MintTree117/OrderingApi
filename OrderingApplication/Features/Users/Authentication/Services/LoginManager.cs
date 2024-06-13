using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Users.Authentication.Types;
using OrderingApplication.Features.Users.Utilities;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.Users.Authentication.Services;

internal sealed class LoginManager( UserConfigCache configCache, UserManager<UserAccount> userManager, IEmailSender emailSender )
{
    const string EmailTokenProvider = "Email";
    const string EmailTokenName = "ConfirmEmailToken";
    
    readonly JwtConfig _jwtConfig = configCache.JwtConfigRules;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly IEmailSender _emailSender = emailSender;
    readonly bool _requiresConfirmedEmail = userManager.Options.SignIn.RequireConfirmedEmail;
    
    // LOGIN
    internal async Task<Reply<LoginInfo>> Login( LoginRequest request )
    {        
        var user = await ValidateLogin( request );
        if (!user)
            return Reply<LoginInfo>.Failure( await _userManager.ProcessAccessFailure( user.Data ) );

        if (await _userManager.Is2FaRequired( user.Data ))
            return await GenerateAndSend2FaCode( user );

        JwtUtils.GenerateAccessToken( user.Data, _jwtConfig, out string token, out ClaimsPrincipal principal );
        return Reply<LoginInfo>.Success( LoginInfo.LoggedIn( token, principal ) );
    }
    async Task<Reply<UserAccount>> ValidateLogin( LoginRequest login )
    {
        IReply validationResult = IReply.Success();
        var validated =
            (await _userManager.FindByEmailOrUsername( login.EmailOrUsername )).OutSuccess( out Reply<UserAccount> userResult ) &&
            (await _userManager.IsAccountValid( userResult, _requiresConfirmedEmail )).OutSuccess( out validationResult ) &&
            await _userManager.CheckPasswordAsync( userResult.Data, login.Password ) &&
            (await ClearAccessFailCount( userResult )).OutSuccess( out validationResult );
        
        return validated
            ? userResult
            : Reply<UserAccount>.Invalid( $"{userResult.GetMessage()} : {validationResult.GetMessage()}" );
    }
    async Task<IReply> ClearAccessFailCount( Reply<UserAccount> user )
    {
        var result = await _userManager.ResetAccessFailedCountAsync( user.Data );
        return result.Succeeded
            ? IReply.Success()
            : IReply.ServerError( $"Failed to reset access count: {result.CombineErrors()}" );
    }
    async Task<Reply<LoginInfo>> GenerateAndSend2FaCode( Reply<UserAccount> user )
    {
        bool generated2Fa =
            (await Set2FaToken( user.Data )).OutSuccess( out IReply problem ) &&
            (await Send2FaEmail( user.Data )).OutSuccess( out problem );
        
        return generated2Fa
            ? Reply<LoginInfo>.Success( LoginInfo.Pending2Fa() )
            : Reply<LoginInfo>.Failure( problem );
    }
    async Task<IReply> Send2FaEmail( UserAccount user )
    {
        const string subject = "Verify your login";
        string code = UserUtils.WebEncode( await _userManager.GenerateTwoFactorTokenAsync( user, EmailTokenProvider ) );
        string body =
            $"""
             <p>Your two-factor verification code is:</p>
             <p style='font-size: 24px; font-weight: bold;'>{code}</p>
             <p>Please enter this code to verify your login. If you did not request this, please ignore this email.</p>
             """;
        string email = UserUtils.GenerateFormattedEmail( user, subject, body );
        return _emailSender.SendHtmlEmail( user.Email ?? string.Empty, subject, email );
    }
    async Task<IReply> Set2FaToken( UserAccount user )
    {
        var token = await _userManager.GenerateTwoFactorTokenAsync( user, EmailTokenProvider );
        var setResult = await _userManager.SetAuthenticationTokenAsync( user, EmailTokenProvider, EmailTokenName, token );
        return setResult.Succeeded
            ? IReply.Success()
            : IReply.ServerError( setResult.CombineErrors() );
    }
    
    // LOGIN 2FA
    internal async Task<Reply<LoginInfo>> Login2Factor( TwoFactorRequest request )
    {
        var user = await Validate2Factor( request );
        if (user)
            return Reply<LoginInfo>.Invalid( await _userManager.ProcessAccessFailure( user.Data ) );

        JwtUtils.GenerateAccessToken( user.Data, _jwtConfig, out string token, out ClaimsPrincipal principal );
        return Reply<LoginInfo>.Success( LoginInfo.LoggedIn( token, principal ) );
    }
    async Task<Reply<UserAccount>> Validate2Factor( TwoFactorRequest twoFactor )
    {
        IReply validationResult = IReply.Success();
        var validated =
            (await _userManager.FindByEmailOrUsername( twoFactor.EmailOrUsername )).OutSuccess( out Reply<UserAccount> userResult ) &&
            (await _userManager.IsAccountValid( userResult, _requiresConfirmedEmail )).OutSuccess( out validationResult ) &&
            (await IsTwoFactorValid( userResult.Data, twoFactor )).OutSuccess( out validationResult );

        return validated
            ? userResult
            : Reply<UserAccount>.Invalid( $"{userResult.GetMessage()} : {validationResult.GetMessage()}" );
    }
    async Task<IReply> IsTwoFactorValid( UserAccount user, TwoFactorRequest twoFactor )
    {
        var code = UserUtils.WebDecode( twoFactor.Code );
        var valid =
            !string.IsNullOrWhiteSpace( twoFactor.Code ) &&
            await _userManager.VerifyTwoFactorTokenAsync( user, EmailTokenProvider, code );
            
        return valid
            ? IReply.Success()
            : IReply.Invalid( "Access token is invalid." );
    }
    
    // LOGIN RECOVERY
    internal async Task<Reply<string>> LoginRecovery( LoginRecoveryRequest request )
    {
        var user = await ValidateRecoveryLogin( request );
        if (!user)
            return Reply<string>.NotFound( await _userManager.ProcessAccessFailure( user.Data ) );
        
        JwtUtils.GenerateAccessToken( user.Data, _jwtConfig, out string token, out ClaimsPrincipal principal );
        return Reply<string>.Success( token );
    }
    async Task<Reply<UserAccount>> ValidateRecoveryLogin( LoginRecoveryRequest request )
    {
        IReply validationResult = IReply.Success();

        bool validated =
            (await _userManager.FindByEmailOrUsername( request.EmailOrUsername )).OutSuccess( out Reply<UserAccount> userResult ) &&
            (await _userManager.IsAccountValid( userResult, _requiresConfirmedEmail )).OutSuccess( out validationResult ) &&
            (await ClearAccessFailCount( userResult )).OutSuccess( out validationResult );

        return validated
            ? userResult
            : Reply<UserAccount>.Invalid( $"{userResult.GetMessage()} : {validationResult.GetMessage()}" );
    }
}
