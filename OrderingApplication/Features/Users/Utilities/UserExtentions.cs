using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;

namespace OrderingApplication.Features.Users.Utilities;

internal static class UserExtentions
{
    internal static async Task<IReply> IsAccountValid( this UserManager<UserAccount> userManager, Reply<UserAccount> user, bool requiresConfirmedEmail )
    {
        bool emailConfirmed = !requiresConfirmedEmail || await userManager.IsEmailConfirmedAsync( user.Data );

        if (!emailConfirmed)
            return IReply.Invalid( "Your account is not confirmed. Please check your email for a confirmation link." );

        if (await userManager.IsLockedOutAsync( user.Data ))
            return IReply.Unauthorized( "Your account is locked. Please try again later." );

        return IReply.Success();
    }
    internal static async Task<bool> Is2FaRequired( this UserManager<UserAccount> userManager, UserAccount user ) =>
        userManager.SupportsUserTwoFactor && await userManager.GetTwoFactorEnabledAsync( user );
    internal static async Task<IReply> ProcessAccessFailure( this UserManager<UserAccount> userManager, UserAccount? account )
    {
        const int maxAccessFails = 5;
        const int lockoutHours = 1;
        
        if (account is null)
            return IReply.UserNotFound();

        var updateResult = await userManager.AccessFailedAsync( account );
        if (!updateResult.Succeeded)
            return IReply.ServerError( updateResult.CombineErrors() );

        if (await userManager.GetAccessFailedCountAsync( account ) < maxAccessFails)
            return IReply.Success();

        var lockoutSet = await userManager.SetLockoutEndDateAsync( account, DateTime.Now + TimeSpan.FromHours( lockoutHours ) );
        return lockoutSet.Succeeded
            ? IReply.Success()
            : IReply.ServerError();
    }
    internal static async Task<Reply<UserAccount>> FindById( this UserManager<UserAccount> manager, string id )
    {
        var user = await manager.Users.FirstOrDefaultAsync( u => u.Id == id );
        return user is not null
            ? Reply<UserAccount>.Success( user )
            : Reply<UserAccount>.UserNotFound();
    }
    internal static async Task<Reply<UserAccount>> FindByEmail( this UserManager<UserAccount> manager, string email )
    {
        var user = await manager.FindByEmailAsync( email );
        return user is not null
            ? Reply<UserAccount>.Success( user )
            : Reply<UserAccount>.UserNotFound();
    }
    internal static async Task<Reply<UserAccount>> FindByEmailOrRecoveryEmail( this UserManager<UserAccount> manager, string email )
    {
        var user = await manager.Users.FirstOrDefaultAsync( u => u.Email == email || u.TwoFactorEmail == email );
        return user is not null
            ? Reply<UserAccount>.Success( user )
            : Reply<UserAccount>.UserNotFound();
    }
    internal static async Task<Reply<UserAccount>> FindByEmailOrUsername( this UserManager<UserAccount> manager, string emailOrUsername )
    {
        var user = await manager.Users.FirstOrDefaultAsync(
            c => c.Email == emailOrUsername || c.UserName == emailOrUsername );
        return user is not null
            ? Reply<UserAccount>.Success( user )
            : Reply<UserAccount>.UserNotFound();
    }
    
    internal static string CombineErrors( this IdentityResult result )
    {
        StringBuilder builder = new();
        foreach ( IdentityError e in result.Errors )
            builder.Append( $"IdentityErrorCode: {e.Code} : Description: {e.Description}" );
        return builder.ToString();
    }
    internal static bool SucceedsOut( this IdentityResult result, out IdentityResult outResult )
    {
        outResult = result;
        return result.Succeeded;
    }
    internal static bool Fails( this IdentityResult result, out IdentityResult outResult )
    {
        outResult = result;
        return !result.Succeeded;
    }
    internal static bool SucceedsOut( this IdentityResult result ) => result.Succeeded;
    internal static bool Fails( this IdentityResult result ) => !result.Succeeded;
}