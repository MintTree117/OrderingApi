using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Account.Login.Types;
using OrderingApplication.Features.Account.Utilities;
using OrderingDomain.Account;
using OrderingDomain.Optionals;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.Account.Login.Services;

internal sealed class LoginRecoverySystem( IdentityConfigCache configCache, UserManager<UserAccount> userManager, IEmailSender emailSender )
{
    const string TokenProvider = "Email";
    const string TokenPurpose = "RecoveryLogin";
    
    readonly JwtConfig _jwtConfig = configCache.JwtConfigRules;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly IEmailSender _emailSender = emailSender;
    
    internal async Task<Reply<bool>> SendRecoveryEmail( string email )
    {
        Reply<UserAccount> userReply = await _userManager.FindByEmail( email );
        if (!userReply.IsSuccess)
            return IReply.None( "Email did not match any users." );
        return await SendRecoveryEmail( userReply.Data );
    }
    internal async Task<Reply<LoginResponse>> LoginRecovery( LoginRecoveryRequest request )
    {
        Reply<UserAccount> userReply = await _userManager.FindByEmailOrUsername( request.EmailOrUsername );
        if (!userReply.IsSuccess)
            return Reply<LoginResponse>.None( "Email or Username did not match any users." );

        if (!await _userManager.VerifyUserTokenAsync( userReply.Data, TokenProvider, TokenPurpose, request.RecoveryCode ))
            return Reply<LoginResponse>.None( "Invalid recovery code." );

        if (await Utils.Is2FaRequired( _userManager, userReply.Data ))
            return Reply<LoginResponse>.With( LoginResponse.Pending2Fa() );
        
        string jwt = JwtUtils.GenerateAccessToken( userReply.Data, _jwtConfig );
        return Reply<LoginResponse>.With( LoginResponse.LoggedIn( jwt ) );
    }

    async Task<Reply<bool>> SendRecoveryEmail( UserAccount user )
    {
        const string header = "Recovery Login";
        string recoveryCode = IdentityUtils.Encode( await _userManager.GenerateUserTokenAsync( user, TokenProvider, TokenPurpose ) );
        string body = $@"
                    <p>Your recovery login code is:</p>
                    <p style='font-size: 24px; font-weight: bold;'>{recoveryCode}</p>
                    <p>Please enter this code to login. If you did not request this, please ignore this email.</p>
                    <p>Please note, if you have two-factor authentication enabled, you will still need to pass that authentication step. This step may also be recovered with recovery tokens.</p>
                    <p>Best regards,<br/>The Team</p>";
        body = IdentityUtils.GenerateFormattedEmail( user, header, body );
        return _emailSender.SendHtmlEmail( user.Email ?? string.Empty, header, body );
    }
}