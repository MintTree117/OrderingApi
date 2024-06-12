using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Account.Security.Types;
using OrderingApplication.Features.Account.Utilities;
using OrderingDomain.Account;
using OrderingDomain.Optionals;

namespace OrderingApplication.Features.Account.Security;

internal sealed class AccountSecurityManager( IdentityConfigCache identityConfigCache, UserManager<UserAccount> userManager )
{
    readonly IdentityConfigCache _configCache = identityConfigCache;
    readonly UserManager<UserAccount> _userManager = userManager;

    internal async Task<Reply<ViewSecurityResponse>> GetSecuritySettings( string userId )
    {
        Reply<UserAccount> userReply = await _userManager.FindById( userId );
        return userReply.IsSuccess
            ? Reply<ViewSecurityResponse>.With( new ViewSecurityResponse( userReply.Data.TwoFactorEnabled, userReply.Data.TwoFactorEmail ) )
            : Reply<ViewSecurityResponse>.None( "User not found." );
    }
    internal async Task<Reply<bool>> UpdatePassword( string userId, UpdatePasswordRequest request )
    {
        Reply<UserAccount> userReply = await _userManager.FindById( userId );
        return userReply.IsSuccess
            ? await TryUpdatePassword( userReply.Data, request )
            : IReply.None( "User not found." );
    }
    internal async Task<Reply<bool>> Update2Fa( string userId, Update2FaRequest request )
    {
        Reply<UserAccount> userReply = await _userManager.FindById( userId );
        if (!userReply.IsSuccess)
            return Reply<bool>.None( "User not found." );
        
        UserAccount user = userReply.Data;

        if (request.TwoFactorEmail == user.Email)
            return IReply.None( "Two Factor Email cannot be the same as your primary email." );
        
        user.TwoFactorEnabled = request.IsEnabled;
        user.TwoFactorEmail = request.TwoFactorEmail;

        IdentityResult updateResult = await _userManager.UpdateAsync( user );
        return updateResult.Succeeded
            ? IReply.Okay()
            : IReply.None( "Failed to save changes to two factor." );
    }

    async Task<Reply<bool>> TryUpdatePassword( UserAccount user, UpdatePasswordRequest request )
    {
        if (string.IsNullOrWhiteSpace( request.NewPassword ))
            return IReply.None( "No replacement password provided." );

        if (IdentityUtils.ValidatePassword( request.NewPassword, _configCache.PasswordConfigRules ).Fails( out var validated ))
            return validated;

        return (await _userManager.ChangePasswordAsync( user, request.OldPassword, request.NewPassword ))
            .Succeeds( out IdentityResult result )
                ? IReply.Okay()
                : IReply.None( result.CombineErrors() );
    }
}