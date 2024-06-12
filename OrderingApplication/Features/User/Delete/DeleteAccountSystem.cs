using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using OrderingApplication.Extentions;
using OrderingDomain.Account;
using OrderingDomain.ReplyTypes;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.User.Delete;

internal sealed class DeleteAccountSystem( UserManager<UserAccount> users, IEmailSender emailSender )
{
    readonly UserManager<UserAccount> _users = users;
    readonly IEmailSender _emailSender = emailSender;
    
    internal async Task<IReply> DeleteIdentity( ClaimsPrincipal claims, string password )
    {
        UserAccount? user = await _users.FindByIdAsync( claims.UserId() );
        if (user is null)
            return IReply.UserNotFound();

        if (!await _users.CheckPasswordAsync( user, password ))
            return IReply.InvalidPassword();

        IdentityResult deleteResult = await _users.DeleteAsync( user );
        return deleteResult.Succeeded
            ? SendDeletionEmail( user, _emailSender )
            : IReply.ServerError();
    }
    
    static IReply SendDeletionEmail( UserAccount user, IEmailSender sender )
    {
        const string subject = "Account Deleted";
        string body =
            $"""
             <!DOCTYPE html>
             <html lang='en'>
             <head>
                 <meta charset='UTF-8'>
                 <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                 <title>{subject}</title>
             </head>
             <body style='font-family: Arial, sans-serif;'>
                 <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                     <h2 style='color: #333;'>{subject}</h2>
                     <p>Dear {user.UserName},</p>
                     <p>We regret to see you go. This is to confirm that you have successfully deleted your account. If this was a mistake, please contact our support team as soon as possible.</p>
                     <p>If you have any feedback or questions, feel free to reach out to us.</p>
                     <p>Best regards,<br/>The Team</p>
                 </div>
             </body>
             </html>
             """;
        return sender.SendHtmlEmail( user.Email ?? string.Empty, subject, body );
    }
}