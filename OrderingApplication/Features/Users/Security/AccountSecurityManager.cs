using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Users.Security.Types;
using OrderingApplication.Features.Users.Utilities;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;

namespace OrderingApplication.Features.Users.Security;

internal sealed class AccountSecurityManager( UserManager<UserAccount> userManager, ILogger<AccountSecurityManager> logger )
    : BaseService<AccountSecurityManager>( logger )
{
    readonly UserManager<UserAccount> _userManager = userManager;

    internal async Task<Reply<ViewSecurityResponse>> GetSecuritySettings( string userId )
    {
        var userReply = await _userManager.FindById( userId );
        return userReply
            ? Reply<ViewSecurityResponse>.Success( new ViewSecurityResponse( userReply.Data.TwoFactorEnabled, userReply.Data.TwoFactorEmail ) )
            : Reply<ViewSecurityResponse>.UserNotFound();
    }
    internal async Task<IReply> UpdatePassword( string userId, UpdatePasswordRequest request )
    {
        var userReply = await _userManager.FindById( userId );
        if (!userReply)
            return IReply.UserNotFound();

        var passwordReply = await TryUpdatePassword( userReply.Data, request );
        return passwordReply;
    }
    internal async Task<IReply> Disable2Fa( string userId )
    {
        var userReply = await _userManager.FindById( userId );
        if (!userReply.Succeeded)
            return IReply.UserNotFound();

        userReply.Data.TwoFactorEnabled = false;
        userReply.Data.TwoFactorEmail = null;

        var updateResult = await _userManager.UpdateAsync( userReply.Data );
        LogIfErrorResult( updateResult );
        return updateResult.Succeeded
            ? IReply.Success()
            : IReply.ChangesNotSaved();
    }
    internal async Task<IReply> Update2Fa( string userId, Update2FaRequest request )
    {
        var userReply = await _userManager.FindById( userId );
        if (!userReply.Succeeded)
            return IReply.UserNotFound();
        
        var user = userReply.Data;
        
        if (string.IsNullOrEmpty( request.TwoFactorEmail ))
            return IReply.BadRequest( "A valid two-factor email address is required." );
        if (request.TwoFactorEmail == user.Email)
            return IReply.Conflict( "Two Factor Email cannot be the same as your primary email." );

        user.TwoFactorEnabled = true;
        user.TwoFactorEmail = request.TwoFactorEmail;
        
        var updateResult = await _userManager.UpdateAsync( user );
        LogIfErrorResult( updateResult );
        return updateResult.Succeeded
            ? IReply.Success()
            : IReply.ChangesNotSaved();
    }

    async Task<IReply> TryUpdatePassword( UserAccount user, UpdatePasswordRequest request )
    {
        var changed = (await _userManager.ChangePasswordAsync( user, request.OldPassword, request.NewPassword ))
            .SucceedsOut( out IdentityResult result );
        LogIfErrorResult( result );
        return changed
            ? IReply.Success()
            : IReply.ChangesNotSaved( result.CombineErrors() );
    }
}