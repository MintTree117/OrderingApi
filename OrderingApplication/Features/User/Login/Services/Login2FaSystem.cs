using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.User.Login.Types;
using OrderingApplication.Features.User.Utilities;
using OrderingDomain.Account;
using OrderingDomain.ReplyTypes;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.User.Login.Services;

internal sealed class Login2FaSystem( AccountConfig config, UserManager<UserAccount> userManager, IEmailSender emailSender )
{
    readonly JwtConfig _jwtConfig = config.JwtConfigRules;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly IEmailSender _emailSender = emailSender;
    readonly bool _requiresConfirmedEmail = userManager.Options.SignIn.RequireConfirmedEmail;
    
    internal async Task<Reply<TwoFactorResponse>> Login2Factor( TwoFactorRequest request )
    {
        Reply<UserAccount> user = await Validate2Factor( request );

        if (!user.Succeeded)
            return Reply<TwoFactorResponse>.Failure(
                await _userManager.ProcessAccessFailure( user.Data ) );

        string token = JwtUtils.GenerateAccessToken( user.Data, _jwtConfig );
        return Reply<TwoFactorResponse>.Success(
            TwoFactorResponse.Authenticated( token ) );
    }
    internal async Task<Reply<LoginResponse>> HandleRequires2Fa( Reply<UserAccount> user )
    {
        bool generated2Fa =
            (await Set2FaToken( user.Data )).OutSuccess( out Reply<bool> problem ) &&
            (await Send2FaEmail( user.Data )).OutSuccess( out problem );

        return generated2Fa
            ? Reply<LoginResponse>.Success( LoginResponse.Pending2Fa() )
            : Reply<LoginResponse>.Failure( problem );
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
    async Task<Reply<bool>> Set2FaToken( UserAccount user )
    {
        IdentityResult result = await _userManager.SetAuthenticationTokenAsync( user, "Email", "Two Factor Token", await _userManager.GenerateTwoFactorTokenAsync( user, "Email" ) );

        return result.Succeeded
            ? IReply.Success()
            : IReply.Fail( "Failed to set authentication token." );
    }
}