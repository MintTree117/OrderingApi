using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Account.Profile;
using OrderingApplication.Utilities;
using OrderingDomain.Optionals;

namespace OrderingApplication.Features.Account.Delete;

internal static class AccountDeleteEndpoints
{
    internal static void MapAccountDeleteEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapDelete( "api/account/delete", static async ( [FromQuery] string password, HttpContext http, AccountProfileManager system ) =>
            await DeleteAccount( password, http, system ) )
            .RequireAuthorization( Consts.DefaultPolicy );
    }

    static async Task<IResult> DeleteAccount( string password, HttpContext http, AccountProfileManager system )
    {
        await http.SignOutAsync( CookieAuthenticationDefaults.AuthenticationScheme );
        Reply<bool> deleteReply = await system.DeleteIdentity( http.UserId(), password );
        if (deleteReply.IsSuccess)
            await http.SignOutAsync( CookieAuthenticationDefaults.AuthenticationScheme );
        return deleteReply.GetIResult();
    }
}