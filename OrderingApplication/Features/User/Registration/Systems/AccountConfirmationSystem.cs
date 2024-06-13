using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.User.Registration.Types;
using OrderingApplication.Features.User.Utilities;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.User.Registration.Systems;

internal sealed class AccountConfirmationSystem( AccountConfig config, UserManager<UserAccount> userManager, IEmailSender emailSender )
{
    readonly AccountConfig _config = config;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly IEmailSender _emailSender = emailSender;

    internal async Task<Reply<bool>> SendEmailConfirmationLink( string email )
    {
        // VALIDATE
        Reply<UserAccount> userReply = await ValidateUser( email );
        if (!userReply.Succeeded)
            return IReply.Fail( $"Failed to send confirmation email. {userReply.GetMessage()}" );

        // SEND EMAIL
        Reply<bool> emailResult = await SendEmailConfirmationEmail( userReply.Data );
        return emailResult;
    }
    internal async Task<Reply<bool>> ConfirmEmail( ConfirmAccountEmailRequest request )
    {
        // VALIDATE
        Reply<UserAccount> userReply = await ValidateUser( request.Email );
        if (!userReply.Succeeded)
            return IReply.Fail( $"Failed to confirm email. {userReply.GetMessage()}" );

        // CONFIRM EMAIL
        Reply<bool> confirmResult = await ConfirmEmail( userReply.Data, request.Code );
        return confirmResult.Succeeded
            ? IReply.Success()
            : IReply.Fail( "Email confirmation code is invalid." );
    }

    async Task<Reply<UserAccount>> ValidateUser( string email )
    {
        // FIND USER
        Reply<UserAccount> userReply = await _userManager.FindByEmail( email );
        if (!userReply.Succeeded)
            return Reply<UserAccount>.Failure( $"User Not Found {userReply.GetMessage()}" );

        // CHECK CONFIRMED
        return await _userManager.IsEmailConfirmedAsync( userReply.Data )
            ? Reply<UserAccount>.Failure( "Email is already confirmed." )
            : Reply<UserAccount>.Success( userReply.Data );
    }
    async Task<Reply<bool>> SendEmailConfirmationEmail( UserAccount user )
    {
        string code = IdentityUtils.WebEncode( await _userManager.GenerateEmailConfirmationTokenAsync( user ) );
        string body = GenerateConfirmEmailBody( user, code, _config.ConfirmEmailPage );
        return _emailSender
               .SendHtmlEmail( user.Email ?? string.Empty, "Confirm your email", body )
               .OutSuccess( out Reply<bool> emailResult )
            ? IReply.Success()
            : IReply.Fail( emailResult );
    }
    async Task<Reply<bool>> ConfirmEmail( UserAccount user, string code )
    {
        IdentityResult result = await _userManager.ConfirmEmailAsync( user, IdentityUtils.WebDecode( code ) );
        return result.Succeeded
            ? IReply.Success()
            : IReply.Fail(result.CombineErrors());
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