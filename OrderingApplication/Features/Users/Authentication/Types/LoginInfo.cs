using System.Security.Claims;

namespace OrderingApplication.Features.Users.Authentication.Types;

internal readonly record struct LoginInfo(
    bool IsPending2Fa,
    string? AccessToken,
    ClaimsPrincipal? ClaimsPrincipal )
{
    internal static LoginInfo LoggedIn( string token, ClaimsPrincipal principal ) =>
        new( false, token, principal );
    internal static LoginInfo Pending2Fa() =>
        new( true, null, null );
}