using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Users.Authentication.Types;
using OrderingApplication.Features.Users.Utilities;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingInfrastructure.Email;
using OrderingInfrastructure.Features.Account.Repositories;

namespace OrderingApplication.Features.Users.Authentication.Services;

internal sealed class LoginManager( AccountConfig config, UserManager<UserAccount> userManager, IEmailSender emailSender, ISessionRepository sessions )
{
    readonly JwtConfig _jwtConfig = config.JwtConfigRules;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly IEmailSender _emailSender = emailSender;
    readonly bool _requiresConfirmedEmail = userManager.Options.SignIn.RequireConfirmedEmail;
    
    // LOGIN
    internal async Task<Reply<LoginModel>> Login( LoginRequest request )
    {        
        var user = await ValidateLogin( request );
        if (!user)
            return Reply<LoginModel>.Failure( await _userManager.ProcessAccessFailure( user.Data ) );

        if (await _userManager.Is2FaRequired( user.Data ))
            return await GenerateAndSend2FaCode( user );

        JwtUtils.GenerateAccessToken( user.Data, _jwtConfig, out string token, out ClaimsPrincipal principal );
        return Reply<LoginModel>.Success( LoginModel.LoggedIn( token, principal ) );
    }
    async Task<Reply<UserAccount>> ValidateLogin( LoginRequest login )
    {
        Reply<bool> validationResult = IReply.Success();

        bool validated =
            (await _userManager.FindByEmailOrUsername( login.EmailOrUsername )).OutSuccess( out Reply<UserAccount> userResult ) &&
            (await _userManager.IsAccountValid( userResult, _requiresConfirmedEmail )).OutSuccess( out validationResult ) &&
            await _userManager.CheckPasswordAsync( userResult.Data, login.Password ) &&
            (await ClearAccessFailCount( userResult )).OutSuccess( out validationResult );

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
    async Task<Reply<LoginModel>> GenerateAndSend2FaCode( Reply<UserAccount> user )
    {
        bool generated2Fa =
            (await Set2FaToken( user.Data )).OutSuccess( out Reply<bool> problem ) &&
            (await Send2FaEmail( user.Data )).OutSuccess( out problem );
        
        return generated2Fa
            ? Reply<LoginModel>.Success( LoginModel.Pending2Fa() )
            : Reply<LoginModel>.Failure( problem );
    }
    async Task<Reply<bool>> Send2FaEmail( UserAccount user )
    {
        const string header = "Verify your login";
        string code = IdentityUtils.WebEncode( await _userManager.GenerateTwoFactorTokenAsync( user, "Email" ) );
        string body = $@"
                    <p>Your two-factor verification code is:</p>
                    <p style='font-size: 24px; font-weight: bold;'>{code}</p>
                    <p>Please enter this code to verify your login. If you did not request this, please ignore this email.</p>
                    <p>Best regards,<br/>The Team</p>";
        body = IdentityUtils.GenerateFormattedEmail( user, header, body );
        return _emailSender.SendHtmlEmail( user.Email ?? string.Empty, header, body );
    }
    async Task<Reply<bool>> Set2FaToken( UserAccount user )
    {
        IdentityResult result = await _userManager.SetAuthenticationTokenAsync( user, "Email", "Two Factor Token", await _userManager.GenerateTwoFactorTokenAsync( user, "Email" ) );

        return result.Succeeded
            ? IReply.Success()
            : IReply.Fail( "Failed to set authentication token." );
    }
    
    // LOGIN 2FA
    internal async Task<Reply<LoginModel>> Login2Factor( TwoFactorRequest request )
    {
        var user = await Validate2Factor( request );
        if (user)
            return Reply<LoginModel>.Failure( await _userManager.ProcessAccessFailure( user.Data ) );

        JwtUtils.GenerateAccessToken( user.Data, _jwtConfig, out string token, out ClaimsPrincipal principal );
        return Reply<LoginModel>.Success( LoginModel.LoggedIn( token, principal ) );
    }
    async Task<Reply<UserAccount>> Validate2Factor( TwoFactorRequest twoFactor )
    {
        Reply<bool> validationResult = IReply.Success();
        bool validated =
            (await _userManager.FindByEmailOrUsername( twoFactor.EmailOrUsername )).OutSuccess( out Reply<UserAccount> userResult ) &&
            (await _userManager.IsAccountValid( userResult, _requiresConfirmedEmail )).OutSuccess( out validationResult ) &&
            (await IsTwoFactorValid( userResult.Data, twoFactor )).OutSuccess( out validationResult );

        return validated
            ? userResult
            : Reply<UserAccount>.Failure( $"{userResult.GetMessage()} : {validationResult.GetMessage()}" );
    }
    async Task<Reply<bool>> IsTwoFactorValid( UserAccount user, TwoFactorRequest twoFactor )
    {
        bool valid =
            !string.IsNullOrWhiteSpace( twoFactor.Code ) &&
            await _userManager.VerifyTwoFactorTokenAsync( user, "Email", twoFactor.Code );

        return valid
            ? IReply.Success()
            : IReply.Fail( "Access token is invalid." );
    }
    
    // LOGIN RECOVERY
    internal async Task<Reply<string>> LoginRecovery( LoginRecoveryRequest request )
    {
        var user = await ValidateRecoveryLogin( request );
        if (!user)
            return Reply<string>.Failure( await _userManager.ProcessAccessFailure( user.Data ) );
        
        JwtUtils.GenerateAccessToken( user.Data, _jwtConfig, out string token, out ClaimsPrincipal principal );
        return Reply<string>.Success( token );
    }
    async Task<Reply<UserAccount>> ValidateRecoveryLogin( LoginRecoveryRequest request )
    {
        Reply<bool> validationResult = IReply.Success();

        bool validated =
            (await _userManager.FindByEmailOrUsername( request.EmailOrUsername )).OutSuccess( out Reply<UserAccount> userResult ) &&
            (await _userManager.IsAccountValid( userResult, _requiresConfirmedEmail )).OutSuccess( out validationResult ) &&
            (await ClearAccessFailCount( userResult )).OutSuccess( out validationResult );

        return validated
            ? userResult
            : Reply<UserAccount>.Failure( $"{userResult.GetMessage()} : {validationResult.GetMessage()}" );
    }
}
