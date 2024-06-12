using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Account.Login.Types;
using OrderingApplication.Features.Account.Utilities;
using OrderingDomain.Account;
using OrderingDomain.Optionals;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.Account.Login.Services;

internal sealed class Login2FaSystem( IdentityConfigCache configCache, UserManager<UserAccount> userManager, IEmailSender emailSender, Login2FaSystem faSystem )
{
    readonly JwtConfig _jwtConfig = configCache.JwtConfigRules;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly IEmailSender _emailSender = emailSender;
    readonly bool _requiresConfirmedEmail = userManager.Options.SignIn.RequireConfirmedEmail;
    
    internal async Task<Reply<TwoFactorResponse>> Login2Factor( TwoFactorRequest request )
    {
        Reply<UserAccount> user = await Validate2Factor( request );

        if (!user.IsSuccess)
            return Reply<TwoFactorResponse>.None(
                await Utils.ProcessAccessFailure( _userManager, user.Data, user.Message() ) );

        string token = JwtUtils.GenerateAccessToken( user.Data, _jwtConfig );
        return Reply<TwoFactorResponse>.With(
            TwoFactorResponse.Authenticated( token ) );
    }
    internal async Task<Reply<LoginResponse>> HandleRequires2Fa( Reply<UserAccount> user )
    {
        bool generated2Fa =
            (await Set2FaToken( user.Data )).Succeeds( out Reply<bool> problem ) &&
            (await Send2FaEmail( user.Data )).Succeeds( out problem );

        return generated2Fa
            ? Reply<LoginResponse>.With( LoginResponse.Pending2Fa() )
            : Reply<LoginResponse>.None( problem );
    }
    async Task<Reply<bool>> Send2FaEmail( UserAccount user )
    {
        const string header = "Verify your login";
        string code = IdentityUtils.Encode( await _userManager.GenerateTwoFactorTokenAsync( user, "Email" ) );
        string body = $@"
                    <p>Your two-factor verification code is:</p>
                    <p style='font-size: 24px; font-weight: bold;'>{code}</p>
                    <p>Please enter this code to verify your login. If you did not request this, please ignore this email.</p>
                    <p>Best regards,<br/>The Team</p>";
        body = IdentityUtils.GenerateFormattedEmail( user, header, body );
        return _emailSender.SendHtmlEmail( user.Email ?? string.Empty, header, body );
    }
    async Task<Reply<UserAccount>> Validate2Factor( TwoFactorRequest twoFactor )
    {
        Reply<bool> validationResult = IReply.Okay();
        bool validated =
            (await _userManager.FindByEmailOrUsername( twoFactor.EmailOrUsername )).Succeeds( out Reply<UserAccount> userResult ) &&
            (await Utils.IsAccountValid( _userManager, userResult, _requiresConfirmedEmail )).Succeeds( out validationResult ) &&
            (await IsTwoFactorValid( userResult.Data, twoFactor )).Succeeds( out validationResult );

        return validated
            ? userResult
            : Reply<UserAccount>.None( $"{userResult.Message()} : {validationResult.Message()}" );
    }
    async Task<Reply<bool>> IsTwoFactorValid( UserAccount user, TwoFactorRequest twoFactor )
    {
        bool valid =
            !string.IsNullOrWhiteSpace( twoFactor.Code ) &&
            await _userManager.VerifyTwoFactorTokenAsync( user, "Email", twoFactor.Code );

        return valid
            ? IReply.Okay()
            : IReply.None( "Access token is invalid." );
    }
    async Task<Reply<bool>> Set2FaToken( UserAccount user )
    {
        IdentityResult result = await _userManager.SetAuthenticationTokenAsync( user, "Email", "Two Factor Token", await _userManager.GenerateTwoFactorTokenAsync( user, "Email" ) );

        return result.Succeeded
            ? IReply.Okay()
            : IReply.None( "Failed to set authentication token." );
    }
}