using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Users.Utilities;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.Users.Registration.Systems;

internal static class Utils
{
    internal static async Task<IReply> SendEmailConfirmationEmail( UserAccount user, UserManager<UserAccount> manager, IEmailSender emailer, string returnPage )
    {
        const string subject = "Confirm Your Email";
        var code = UserUtils.WebEncode( await manager.GenerateEmailConfirmationTokenAsync( user ) );
        var body = GenerateConfirmEmailBody( user, subject, code, returnPage );
        var sent = emailer
            .SendHtmlEmail( user.Email ?? string.Empty, subject, body )
            .OutSuccess( out Reply<bool> emailResult );
        return sent
            ? IReply.Success()
            : IReply.Fail( emailResult );
    }

    static string GenerateConfirmEmailBody( UserAccount user, string subject, string code, string returnUrl )
    {
        string link = $"{returnUrl}?email={user.Email}&code={code}";
        string customBody =
            $"""
             <p>Thank you for registering with us. Please click the link below to confirm your email address:</p>
             <p>
                 <a href='{link}' style='display: inline-block; padding: 10px 20px; font-size: 16px; color: #fff; background-color: #007BFF; text-decoration: none; border-radius: 5px;'>Confirm Email</a>
             </p>
             <p>If you did not create an account, please ignore this email.</p>
             """;
        string body = UserUtils.GenerateFormattedEmail( user, subject, customBody );
        return body;
    }
}