using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Utilities;

namespace OrderingApplication.Features.Users.Delete;

internal static class AccountDeleteEndpoints
{
    internal static void MapAccountDeleteEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapDelete( "api/account/delete",
            static async ( [FromQuery] string password, HttpContext http, DeleteAccountSystem system ) =>
            await DeleteAccount( password, http, system ) ).RequireAuthorization( Consts.DefaultPolicy );
    }
    static async Task<IResult> DeleteAccount( string password, HttpContext http, DeleteAccountSystem system )
    {
        await http.SignOutAsync( CookieAuthenticationDefaults.AuthenticationScheme );

        bool deleted = (await system.DeleteIdentity( http.User, password ))
            .OutSuccess( out var deleteReply );

        if (deleted)
            await http.SignOutAsync( CookieAuthenticationDefaults.AuthenticationScheme );
        return deleteReply.GetIResult();
    }
}