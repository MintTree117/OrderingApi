using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Identity.Types.Password;
using OrderingApplication.Features.Identity.Utilities;
using OrderingDomain.Identity;
using OrderingDomain.Optionals;

namespace OrderingApplication.Features.Identity.Services;

internal sealed class UserSecuritySystem( IdentityConfigCache identityConfigCache, UserManager<UserAccount> um )
{
    readonly IdentityConfigCache _configCache = identityConfigCache;
    readonly UserManager<UserAccount> _userManager = um;

    internal async Task<Reply<bool>> UpdatePassword( string userId, UpdatePasswordRequest request )
    {
        Reply<UserAccount> result = await _userManager.FindById( userId );
        return result.IsSuccess
            ? await ManagePassword( result.Data, request )
            : IReply.None( "User not found." );
    }
    internal async Task<Reply<bool>> UpdateTwoFactor( string userId, bool enabled)
    {
        Reply<UserAccount> user = await _userManager.FindById( userId );
        if (!user.IsSuccess)
            return Reply<bool>.None( "User Not Found." );

        UserAccount u = user.Data;
        
        if (u.TwoFactorEnabled) // clear recovery codes if turning off
            await _userManager.GenerateNewTwoFactorRecoveryCodesAsync( u, 0 );
        
        u.TwoFactorEnabled = enabled;

        return IReply.Okay();
    }

    async Task<Reply<bool>> ManagePassword( UserAccount user, UpdatePasswordRequest request )
    {
        if (string.IsNullOrWhiteSpace( request.NewPassword ))
            return IReply.None( "No replacement password provided." );

        if (IdentityValidationUtils.ValidatePassword( request.NewPassword, _configCache.PasswordConfigRules ).Fails( out var validated ))
            return validated;

        return (await _userManager.ChangePasswordAsync( user, request.OldPassword, request.NewPassword ))
            .Succeeds( out IdentityResult result )
                ? IReply.Okay()
                : IReply.None( result.CombineErrors() );
    }
}