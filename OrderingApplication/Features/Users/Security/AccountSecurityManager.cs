using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Users.Security.Types;
using OrderingApplication.Features.Users.Utilities;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;

namespace OrderingApplication.Features.Users.Security;

internal sealed class AccountSecurityManager( UserManager<UserAccount> userManager )
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
    internal async Task<IReply> Update2Fa( string userId, Update2FaRequest request )
    {
        var userReply = await _userManager.FindById( userId );
        if (!userReply.Succeeded)
            return IReply.UserNotFound();
        
        UserAccount user = userReply.Data;

        if (request.TwoFactorEmail == user.Email)
            return IReply.Conflict( "Two Factor Email cannot be the same as your primary email." );
        
        user.TwoFactorEnabled = request.IsEnabled;
        user.TwoFactorEmail = request.TwoFactorEmail;
        
        var updateResult = await _userManager.UpdateAsync( user );
        return updateResult.Succeeded
            ? IReply.Success()
            : IReply.ChangesNotSaved();
    }

    async Task<IReply> TryUpdatePassword( UserAccount user, UpdatePasswordRequest request )
    {
        var changed = (await _userManager.ChangePasswordAsync( user, request.OldPassword, request.NewPassword ))
            .SucceedsOut( out IdentityResult result );

        return changed
            ? IReply.Success()
            : IReply.ChangesNotSaved( result.CombineErrors() );
    }
}