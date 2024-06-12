using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.User.Profile.Types;
using OrderingApplication.Utilities;

namespace OrderingApplication.Features.User.Profile;

internal static class AccountProfileEndpoints
{
    internal static void MapAccountProfileEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapGet( "api/account/profile/view", static async ( HttpContext http, AccountProfileManager system ) =>
            (await system.ViewProfile( http.UserId() ))
            .GetIResult() ).RequireAuthorization( Consts.DefaultPolicy );
        app.MapPut( "api/account/profile/update", static async ( [FromBody] UpdateProfileRequest request, HttpContext http, AccountProfileManager system ) =>
            (await system.UpdateProfile( http.UserId(), request ))
            .GetIResult() ).RequireAuthorization( Consts.DefaultPolicy );
    }
}