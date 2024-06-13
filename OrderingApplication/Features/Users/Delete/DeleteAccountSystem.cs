using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Users.Utilities;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.Users.Delete;

internal sealed class DeleteAccountSystem( UserManager<UserAccount> users, IEmailSender emailSender )
{
    readonly UserManager<UserAccount> _users = users;
    readonly IEmailSender _emailSender = emailSender;
    
    internal async Task<IReply> DeleteIdentity( ClaimsPrincipal claims, string password )
    {
        var user = await _users.FindByIdAsync( claims.UserId() );
        if (user is null)
            return IReply.UserNotFound();

        if (!await _users.CheckPasswordAsync( user, password ))
            return IReply.InvalidPassword();

        var deleteResult = await _users.DeleteAsync( user );
        return deleteResult.Succeeded
            ? SendDeletionEmail( user, _emailSender )
            : IReply.ServerError();
    }
    
    static IReply SendDeletionEmail( UserAccount user, IEmailSender sender )
    {
        const string subject = "Account Deleted";
        const string customBody = "<p>We regret to see you go. This is to confirm that you have successfully deleted your account. If this was a mistake, please contact our support team as soon as possible.</p>";
        string body = UserUtils.GenerateFormattedEmail( user, subject, customBody );
        return sender.SendHtmlEmail( user.Email ?? string.Empty, subject, body );
    }
}