using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Users.Authentication.Types;
using OrderingApplication.Features.Users.Utilities;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.Users.Authentication.Services;

internal sealed class PasswordResetter( AccountConfig config, UserManager<UserAccount> userManager, IEmailSender emailSender )
{
    readonly AccountConfig _config = config;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly IEmailSender _emailSender = emailSender;
    
    internal async Task<Reply<bool>> ForgotPassword( string email )
    {
        Reply<UserAccount> userReply = await _userManager.FindByEmail( email );
        return userReply.Succeeded
            ? await SendResetEmail( userReply )
            : IReply.Fail( "User not found." );
    }
    internal async Task<Reply<bool>> ResetPassword( ResetPasswordRequest request )
    {
        Reply<UserAccount> userReply = await _userManager.FindByEmail( request.Email );
        if (!userReply.Succeeded)
            return IReply.Fail( "User Not Found" );

        IdentityResult passwordReply = await TryResetPassword( userReply, request );
        return passwordReply.Succeeded
            ? IReply.Success()
            : IReply.Fail( "Internal server error. Failed to reset password." );
    }
    
    async Task<Reply<bool>> SendResetEmail( Reply<UserAccount> user ) =>
        _emailSender
            .SendHtmlEmail( user.Data.Email!, "Reset your password", await GenerateResetEmailBody( user.Data ) )
            .OutSuccess( out Reply<bool> result )
            ? IReply.Success()
            : IReply.Fail( result );
    async Task<string> GenerateResetEmailBody( UserAccount user ) =>
        $"Please reset your password by <a href='{await GenerateResetLink( user )}'>clicking here</a>.";
    async Task<string> GenerateResetLink( UserAccount user ) =>
        $"{_config.ResetPasswordPage}?Email={user.Email}&Code={IdentityUtils.WebEncode( await _userManager.GeneratePasswordResetTokenAsync( user ) )}";
    async Task<IdentityResult> TryResetPassword( Reply<UserAccount> user, ResetPasswordRequest request ) =>
        await _userManager.ResetPasswordAsync( user.Data, IdentityUtils.WebDecode( request.Code ), request.NewPassword );
}