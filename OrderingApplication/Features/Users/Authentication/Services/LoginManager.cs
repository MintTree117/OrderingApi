using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Users.Authentication.Types;
using OrderingApplication.Features.Users.Utilities;
using OrderingApplication.Utilities;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.Users.Authentication.Services;

internal sealed class LoginManager( UserManager<UserAccount> userManager, IEmailSender emailSender, ILogger<LoginManager> logger )
    : BaseService<LoginManager>( logger )
{
    const string EmailTokenProvider = "Email";
    const string EmailTokenName = "ConfirmEmailToken";
    
    readonly JwtConfig _jwtConfig = UserConsts.Instance.JwtConfigRules;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly IEmailSender _emailSender = emailSender;
    readonly bool _requiresConfirmedEmail = userManager.Options.SignIn.RequireConfirmedEmail;
    
    // LOGIN
    internal async Task<Reply<LoginInfo>> Login( LoginRequest request )
    {
        var user = await _userManager.FindByEmailOrUsername( request.EmailOrUsername );
        if (!user)
            return Reply<LoginInfo>.UserNotFound();

        var login = await ValidateLogin( user.Data, request );
        if (!login)
            return await ProcessFailure<LoginInfo>( user.Data, login );

        if (await _userManager.Is2FaRequired( user.Data ))
            return await GenerateAndSend2FaCode( user );

        JwtUtils.GenerateAccessToken( user.Data, _jwtConfig, out string token, out ClaimsPrincipal principal );
        return Reply<LoginInfo>.Success( LoginInfo.LoggedIn( token, principal ) );
    }
    async Task<Reply<bool>> ValidateLogin( UserAccount user, LoginRequest request )
    {
        var validated =
            (await _userManager.IsAccountValid( user, _requiresConfirmedEmail )).OutSuccess( out IReply validationResult ) &&
            await _userManager.CheckPasswordAsync( user, request.Password ) &&
            (await ClearAccessFailCount( user )).OutSuccess( out validationResult );
        
        return validated
            ? IReply.Success()
            : IReply.Invalid( validationResult );
    }
    async Task<IReply> ClearAccessFailCount( UserAccount user )
    {
        var result = await _userManager.ResetAccessFailedCountAsync( user );
        return result.Succeeded
            ? IReply.Success()
            : IReply.ServerError( $"Failed to reset access count: {result.CombineErrors()}" );
    }
    async Task<Reply<LoginInfo>> GenerateAndSend2FaCode( Reply<UserAccount> user )
    {
        if (string.IsNullOrWhiteSpace( user.Data.TwoFactorEmail ))
            return Reply<LoginInfo>.Conflict( "Your two factor email isn't set. The login cannot proceed." );

        bool generated2Fa =
            (await Set2FaToken( user.Data )).OutSuccess( out IReply problem ) &&
            (await Send2FaEmail( user.Data )).OutSuccess( out problem );
        
        LogReplyError( problem );
        
        return generated2Fa
            ? Reply<LoginInfo>.Success( LoginInfo.Pending2Fa() )
            : Reply<LoginInfo>.Failure( problem );
    }
    async Task<IReply> Set2FaToken( UserAccount user )
    {
        var token = await _userManager.GenerateTwoFactorTokenAsync( user, EmailTokenProvider );
        var setResult = await _userManager.SetAuthenticationTokenAsync( user, EmailTokenProvider, EmailTokenName, token );
        return setResult.Succeeded
            ? IReply.Success()
            : IReply.ServerError( setResult.CombineErrors() );
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
        return _emailSender.SendHtmlEmail( user.TwoFactorEmail ?? string.Empty, subject, email );
    }
    
    // LOGIN 2FA
    internal async Task<Reply<LoginInfo>> Login2Factor( TwoFactorRequest request )
    {
        var user = await _userManager.FindByEmailOrUsername( request.EmailOrUsername );
        if (!user)
            return Reply<LoginInfo>.UserNotFound();

        var twoFactor = await Validate2Factor( user.Data, request );
        if (!twoFactor)
            return await ProcessFailure<LoginInfo>( user.Data, twoFactor );

        JwtUtils.GenerateAccessToken( user.Data, _jwtConfig, out string token, out ClaimsPrincipal principal );
        return Reply<LoginInfo>.Success( LoginInfo.LoggedIn( token, principal ) );
    }
    async Task<Reply<bool>> Validate2Factor( UserAccount user, TwoFactorRequest twoFactor )
    {
        var validated =
            (await _userManager.IsAccountValid( user, _requiresConfirmedEmail )).OutSuccess( out IReply validationResult ) &&
            (await IsTwoFactorValid( user, twoFactor )).OutSuccess( out validationResult );

        return validated
            ? IReply.Success()
            : IReply.Unauthorized( validationResult );
    }
    async Task<IReply> IsTwoFactorValid( UserAccount user, TwoFactorRequest twoFactor )
    {
        var code = UserUtils.WebDecode( twoFactor.Code );
        var valid =
            !string.IsNullOrWhiteSpace( twoFactor.Code ) &&
            await _userManager.VerifyTwoFactorTokenAsync( user, EmailTokenProvider, code );
            
        return valid
            ? IReply.Success()
            : IReply.Unauthorized( "Access token is invalid." );
    }
    
    // LOGIN RECOVERY
    internal async Task<Reply<string>> LoginRecovery( LoginRecoveryRequest request )
    {
        var user = await _userManager.FindByEmailOrUsername( request.EmailOrUsername );
        if (!user)
            return Reply<string>.UserNotFound();

        var login = await ValidateRecoveryLogin( user.Data, request );
        if (!login)
            return await ProcessFailure<string>( user.Data, login );
        
        JwtUtils.GenerateAccessToken( user.Data, _jwtConfig, out string token, out ClaimsPrincipal principal );
        return Reply<string>.Success( token );
    }
    async Task<Reply<bool>> ValidateRecoveryLogin( UserAccount user, LoginRecoveryRequest request )
    {
        bool validated =
            (await _userManager.IsAccountValid( user, _requiresConfirmedEmail )).OutSuccess( out IReply validationResult ) &&
            (await ClearAccessFailCount( user )).OutSuccess( out validationResult );

        return validated
            ? IReply.Success()
            : IReply.Unauthorized( validationResult );
    }
    
    // SHARED
    async Task<Reply<T>> ProcessFailure<T>( UserAccount user, IReply failure )
    {
        var processReply = await _userManager.ProcessAccessFailure( user );
        Logger.LogReplyError( processReply );
        return Reply<T>.Unauthorized( "Login Credentials Failed." );
    }
}
