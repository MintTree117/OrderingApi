using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Users.Profile.Types;
using OrderingApplication.Utilities;

namespace OrderingApplication.Features.Users.Profile;

internal static class AccountProfileEndpoints
{
    internal static void MapAccountProfileEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapGet( "api/account/profile/view",
            static async ( HttpContext http, AccountProfileManager manager ) =>
                await GetProfile( http, manager ) ).RequireAuthorization( Consts.DefaultPolicy );

        app.MapPut( "api/account/profile/update",
            static async ( [FromBody] UpdateProfileRequest request, HttpContext http, AccountProfileManager manager ) =>
            await UpdateProfile( request, http, manager ) ).RequireAuthorization( Consts.DefaultPolicy );
    }

    static async Task<IResult> GetProfile( HttpContext http, AccountProfileManager manager )
    {
        var reply = await manager.ViewProfile( http.UserId() );
        return reply.GetIResult();
    }
    static async Task<IResult> UpdateProfile( UpdateProfileRequest request, HttpContext http, AccountProfileManager manager )
    {
        var reply = await manager.UpdateProfile( http.UserId(), request );
        return reply.GetIResult();
    }
}