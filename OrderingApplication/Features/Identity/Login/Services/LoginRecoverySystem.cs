using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Identity.AccountSecurity.Types;
using OrderingApplication.Features.Identity.Utilities;
using OrderingDomain.Identity;
using OrderingDomain.Optionals;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.Identity.Login.Services;

internal sealed class LoginRecoverySystem( IdentityConfigCache configCache, UserManager<UserAccount> userManager, IEmailSender emailSender )
{
    readonly IdentityConfigCache _configCache = configCache;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly IEmailSender _emailSender = emailSender;
    
    internal async Task<Reply<bool>> ForgotPassword( ForgotPasswordRequest request )
    {
        Reply<UserAccount> userReply = await _userManager.FindByEmail( request.Email );
        return userReply.IsSuccess
            ? await SendResetEmail( userReply )
            : IReply.None( "User not found." );
    }
    internal async Task<Reply<bool>> ResetPassword( ResetPasswordRequest request )
    {
        Reply<UserAccount> userReply = await _userManager.FindByEmail( request.Email );
        if (!userReply.IsSuccess)
            return IReply.None( "User Not Found" );

        IdentityResult passwordReply = await TryResetPassword( userReply, request );
        return passwordReply.Succeeded
            ? IReply.Okay()
            : IReply.None( "Internal server error. Failed to reset password." );
    }
    
    async Task<Reply<bool>> SendResetEmail( Reply<UserAccount> user ) =>
        _emailSender
            .SendHtmlEmail( user.Data.Email!, "Reset your password", await GenerateResetEmailBody( user.Data ) )
            .Succeeds( out Reply<bool> result )
            ? IReply.Okay()
            : IReply.None( result );
    async Task<string> GenerateResetEmailBody( UserAccount user ) =>
        $"Please reset your password by <a href='{await GenerateResetLink( user )}'>clicking here</a>.";
    async Task<string> GenerateResetLink( UserAccount user ) =>
        $"{_configCache.ResetPasswordPage}?Email={user.Email}&Code={IdentityValidationUtils.Encode( await _userManager.GeneratePasswordResetTokenAsync( user ) )}";
    async Task<IdentityResult> TryResetPassword( Reply<UserAccount> user, ResetPasswordRequest request ) =>
        await _userManager.ResetPasswordAsync( user.Data, IdentityValidationUtils.Decode( request.Code ), request.NewPassword );
}