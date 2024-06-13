using System.Security.Claims;

namespace OrderingApplication.Features.Users.Authentication.Types;

internal readonly record struct LoginModel(
    bool IsPending2Fa,
    string? AccessToken,
    ClaimsPrincipal? ClaimsPrincipal )
{
    internal static LoginModel LoggedIn( string token, ClaimsPrincipal principal ) =>
        new( false, token, principal );
    internal static LoginModel Pending2Fa() =>
        new( true, null, null );
}