using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.User.Login.Types;
using OrderingApplication.Features.User.Utilities;
using OrderingDomain.Account;
using OrderingDomain.ReplyTypes;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.User.Login.Services;

internal sealed class LoginRecoverySystem( AccountConfig config, UserManager<UserAccount> userManager, IEmailSender emailSender )
{
    const string TokenProvider = "Email";
    const string TokenPurpose = "RecoveryLogin";
    
    readonly JwtConfig _jwtConfig = config.JwtConfigRules;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly IEmailSender _emailSender = emailSender;
    
    internal async Task<Reply<bool>> SendRecoveryEmail( string email )
    {
        Reply<UserAccount> userReply = await _userManager.FindByEmail( email );
        if (!userReply.Succeeded)
            return IReply.Fail( "Email did not match any users." );
        return await SendRecoveryEmail( userReply.Data );
    }
    internal async Task<Reply<LoginResponse>> LoginRecovery( LoginRecoveryRequest request )
    {
        Reply<UserAccount> userReply = await _userManager.FindByEmailOrUsername( request.EmailOrUsername );
        if (!userReply.Succeeded)
            return Reply<LoginResponse>.Failure( "Email or Username did not match any users." );

        if (!await _userManager.VerifyUserTokenAsync( userReply.Data, TokenProvider, TokenPurpose, request.RecoveryCode ))
            return Reply<LoginResponse>.Failure( "Invalid recovery code." );

        if (await _userManager.Is2FaRequired( userReply.Data ))
            return Reply<LoginResponse>.Success( LoginResponse.Pending2Fa() );
        
        string jwt = JwtUtils.GenerateAccessToken( userReply.Data, _jwtConfig );
        return Reply<LoginResponse>.Success( LoginResponse.LoggedIn( jwt ) );
    }

    async Task<Reply<bool>> SendRecoveryEmail( UserAccount user )
    {
        const string header = "Recovery Login";
        string recoveryCode = IdentityUtils.WebEncode( await _userManager.GenerateUserTokenAsync( user, TokenProvider, TokenPurpose ) );
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