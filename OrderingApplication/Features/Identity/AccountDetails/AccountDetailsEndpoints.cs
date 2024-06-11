using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Identity.AccountDetails.Types;

namespace OrderingApplication.Features.Identity.AccountDetails;

internal static class AccountDetailsEndpoints
{
    const string CookiesOrJwt = "CookiesOrJwt";
    
    internal static void MapAccountDetailsEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapGet( "api/account/details/view", static async ( HttpContext http, AccountDetailsManager system ) =>
            (await system.ViewIdentity( http.UserId() ))
            .GetIResult() ).RequireAuthorization( CookiesOrJwt );
        app.MapPost( "api/account/details/update", static async ( [FromBody] UpdateDetailsRequest request, HttpContext http, AccountDetailsManager system ) =>
            (await system.UpdateAccount( http.UserId(), request ))
            .GetIResult() ).RequireAuthorization( CookiesOrJwt );
    }
}