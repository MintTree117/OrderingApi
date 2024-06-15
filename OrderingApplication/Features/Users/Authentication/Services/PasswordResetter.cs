using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Users.Authentication.Types;
using OrderingApplication.Features.Users.Utilities;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.Users.Authentication.Services;

internal sealed class PasswordResetter( UserManager<UserAccount> userManager, IEmailSender emailSender, ILogger<PasswordResetter> logger )
    : BaseService<PasswordResetter>( logger )
{
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly IEmailSender _emailSender = emailSender;
    
    internal async Task<IReply> ForgotPassword( string email )
    {
        var userReply = await _userManager.FindByEmail( email );
        if (!userReply)
            return IReply.UserNotFound();

        var emailReply = await SendResetEmail( userReply.Data );
        LogReplyError( emailReply );
        return emailReply
            ? IReply.Success()
            : IReply.ServerError( emailReply );
    }
    internal async Task<IReply> ResetPassword( ResetPasswordRequest request )
    {
        var userReply = await _userManager.FindByEmailOrRecoveryEmail( request.Email );
        if (!userReply.Succeeded)
            return IReply.UserNotFound();

        var code = UserUtils.WebDecode( request.Code );
        var passwordReply = await _userManager.ResetPasswordAsync( userReply.Data, code, request.NewPassword );
        LogIdentityResultError( passwordReply );
        return passwordReply.Succeeded
            ? IReply.Success()
            : IReply.InvalidPassword( passwordReply.CombineErrors() );
    }

    async Task<Reply<bool>> SendResetEmail( UserAccount user )
    {
        const string subject = "Reset Password";
        string code = UserUtils.WebEncode( await _userManager.GeneratePasswordResetTokenAsync( user ) );
        string returnUrl = $"{UserConsts.Instance.ResetPasswordPage}?Email={user.Email}&Code={code}";
        string body = GenerateResetEmail( user, code, returnUrl, subject );
        return _emailSender
            .SendHtmlEmail( user.Email ?? string.Empty, subject, body )
            .OutSuccess( out Reply<bool> emailResult )
            ? IReply.Success()
            : IReply.Fail( emailResult );
    }
    static string GenerateResetEmail( UserAccount user, string code, string returnUrl, string subject )
    {
        string link = $"{returnUrl}?email={user.Email}&code={code}";
        string customBody =
            $"""
             <p>Please click the link below to reset your password:</p>
             <hr/>
             <p>
                <a href={link}>{link}</a>
             </p>
             <hr/>
             <p>If you did not request this, please ignore this email.</p>
             """;
        string body = UserUtils.GenerateFormattedEmail( user, subject, customBody );
        return body;
    }
}