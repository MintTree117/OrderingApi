using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Account.Profile.Types;

namespace OrderingApplication.Features.Account.Profile;

internal static class AccountProfileEndpoints
{
    const string CookiesOrJwt = "CookiesOrJwt";
    
    internal static void MapAccountDetailsEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapGet( "api/account/details/view", static async ( HttpContext http, AccountProfileManager system ) =>
            (await system.ViewIdentity( http.UserId() ))
            .GetIResult() ).RequireAuthorization( CookiesOrJwt );
        app.MapPut( "api/account/details/update", static async ( [FromBody] UpdateProfileRequest request, HttpContext http, AccountProfileManager system ) =>
            (await system.UpdateAccount( http.UserId(), request ))
            .GetIResult() ).RequireAuthorization( CookiesOrJwt );
    }
}