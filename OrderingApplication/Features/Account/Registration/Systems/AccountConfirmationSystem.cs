using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Account.Registration.Types;
using OrderingApplication.Features.Account.Utilities;
using OrderingDomain.Account;
using OrderingDomain.Optionals;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.Account.Registration.Systems;

internal sealed class AccountConfirmationSystem( IdentityConfigCache configCache, UserManager<UserAccount> userManager, IEmailSender emailSender )
{
    readonly IdentityConfigCache _configCache = configCache;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly IEmailSender _emailSender = emailSender;

    internal async Task<Reply<bool>> SendEmailConfirmationLink( string email )
    {
        // VALIDATE
        Reply<UserAccount> userReply = await ValidateUser( email );
        if (!userReply.IsSuccess)
            return IReply.None( $"Failed to send confirmation email. {userReply.Message()}" );

        // SEND EMAIL
        Reply<bool> emailResult = await SendEmailConfirmationEmail( userReply.Data );
        return emailResult;
    }
    internal async Task<Reply<bool>> ConfirmEmail( ConfirmAccountEmailRequest request )
    {
        // VALIDATE
        Reply<UserAccount> userReply = await ValidateUser( request.Email );
        if (!userReply.IsSuccess)
            return IReply.None( $"Failed to confirm email. {userReply.Message()}" );

        // CONFIRM EMAIL
        Reply<bool> confirmResult = await ConfirmEmail( userReply.Data, request.Code );
        return confirmResult.IsSuccess
            ? IReply.Okay()
            : IReply.None( "Email confirmation code is invalid." );
    }

    async Task<Reply<UserAccount>> ValidateUser( string email )
    {
        // FIND USER
        Reply<UserAccount> userReply = await _userManager.FindByEmail( email );
        if (!userReply.IsSuccess)
            return Reply<UserAccount>.None( $"User Not Found {userReply.Message()}" );

        // CHECK CONFIRMED
        return await _userManager.IsEmailConfirmedAsync( userReply.Data )
            ? Reply<UserAccount>.None( "Email is already confirmed." )
            : Reply<UserAccount>.With( userReply.Data );
    }
    async Task<Reply<bool>> SendEmailConfirmationEmail( UserAccount user )
    {
        string code = IdentityUtils.Encode( await _userManager.GenerateEmailConfirmationTokenAsync( user ) );
        string body = GenerateConfirmEmailBody( user, code, _configCache.ConfirmEmailPage );
        return _emailSender
               .SendHtmlEmail( user.Email ?? string.Empty, "Confirm your email", body )
               .Succeeds( out Reply<bool> emailResult )
            ? IReply.Okay()
            : IReply.None( emailResult );
    }
    async Task<Reply<bool>> ConfirmEmail( UserAccount user, string code )
    {
        IdentityResult result = await _userManager.ConfirmEmailAsync( user, IdentityUtils.Decode( code ) );
        return result.Succeeded
            ? IReply.Okay()
            : IReply.None(result.CombineErrors());
    }
    static string GenerateConfirmEmailBody( UserAccount user, string token, string returnUrl )
    {
        string link = $"{returnUrl}?email={user.Email}&code={token}";
        string html = $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Email Confirmation</title>
            </head>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #333;'>Email Confirmation</h2>
                    <p>Dear {user.UserName},</p>
                    <p>Thank you for registering with us. Please click the link below to confirm your email address:</p>
                    <p>
                        <a href='{link}' style='display: inline-block; padding: 10px 20px; font-size: 16px; color: #fff; background-color: #007BFF; text-decoration: none; border-radius: 5px;'>Confirm Email</a>
                    </p>
                    <p>If you did not create an account, please ignore this email.</p>
                    <p>Best regards,<br/>The Team</p>
                </div>
            </body>
            </html>";
        return html;
    }
}