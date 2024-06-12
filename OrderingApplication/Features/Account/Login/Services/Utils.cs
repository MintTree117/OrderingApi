using Microsoft.AspNetCore.Identity;
using OrderingDomain.Account;
using OrderingDomain.Optionals;

namespace OrderingApplication.Features.Account.Login.Services;

internal static class Utils
{
    internal static async Task<Reply<bool>> IsAccountValid( UserManager<UserAccount> userManager, Reply<UserAccount> user, bool requiresConfirmedEmail )
    {
        bool emailConfirmed = !requiresConfirmedEmail || await userManager.IsEmailConfirmedAsync( user.Data );

        if (!emailConfirmed)
            return Reply<bool>.None( "Your account is not confirmed. Please check your email for a confirmation link." );

        if (await userManager.IsLockedOutAsync( user.Data ))
            return Reply<bool>.None( "Your account is locked. Please try again later." );

        return IReply.Okay();
    }
    internal static async Task<bool> Is2FaRequired( UserManager<UserAccount> userManager, UserAccount user ) =>
        userManager.SupportsUserTwoFactor && await userManager.GetTwoFactorEnabledAsync( user );
    internal static async Task<IReply> ProcessAccessFailure( UserManager<UserAccount> userManager, UserAccount account, string message )
    {
        if (account is null)
            return IReply.None( "User not found." );

        await userManager.AccessFailedAsync( account );

        if (await userManager.GetAccessFailedCountAsync( account ) > 5)
            await userManager.SetLockoutEndDateAsync( account, DateTime.Now + TimeSpan.FromHours( 1 ) );

        return IReply.None( message );
    }
}