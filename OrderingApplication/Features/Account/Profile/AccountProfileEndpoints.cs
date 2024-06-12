using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Account.Profile.Types;
using OrderingApplication.Utilities;

namespace OrderingApplication.Features.Account.Profile;

internal static class AccountProfileEndpoints
{
    internal static void MapAccountDetailsEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapGet( "api/account/profile/view", static async ( HttpContext http, AccountProfileManager system ) =>
            (await system.ViewIdentity( http.UserId() ))
            .GetIResult() ).RequireAuthorization( Consts.DefaultPolicy );
        app.MapPut( "api/account/profile/update", static async ( [FromBody] UpdateProfileRequest request, HttpContext http, AccountProfileManager system ) =>
            (await system.UpdateAccount( http.UserId(), request ))
            .GetIResult() ).RequireAuthorization( Consts.DefaultPolicy );
    }
}