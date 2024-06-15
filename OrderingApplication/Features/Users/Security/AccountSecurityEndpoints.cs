using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Users.Security.Types;
using OrderingApplication.Utilities;

namespace OrderingApplication.Features.Users.Security;

internal static class AccountSecurityEndpoints
{
    internal static void MapAccountSecurityEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapGet( "api/account/security/view",
            static async ( HttpContext http, AccountSecurityManager manager ) =>
                await GetSecuritySettings( http, manager ) ).RequireAuthorization( Consts.DefaultPolicy );
        
        app.MapPut( "api/account/security/updatePassword",
            static async ( [FromBody] UpdatePasswordRequest request, HttpContext http, AccountSecurityManager manager ) =>
            await UpdatePassword( request, http, manager ) ).RequireAuthorization( Consts.DefaultPolicy );

        app.MapPut( "api/account/security/disable2fa",
            static async ( [FromBody] Update2FaRequest request, HttpContext http, AccountSecurityManager manager ) =>
            await UpdateTwoFactor( request, http, manager ) ).RequireAuthorization( Consts.DefaultPolicy );

        app.MapPut( "api/account/security/update2fa",
            static async ( HttpContext http, AccountSecurityManager manager ) =>
            await DisableTwoFactor( http, manager ) ).RequireAuthorization( Consts.DefaultPolicy );
    }

    static async Task<IResult> GetSecuritySettings( HttpContext http, AccountSecurityManager manager )
    {
        var reply = await manager.GetSecuritySettings( http.UserId() );
        return reply.GetIResult();
    }
    static async Task<IResult> UpdatePassword( UpdatePasswordRequest request, HttpContext http, AccountSecurityManager manager )
    {
        var reply = await manager.UpdatePassword( http.UserId(), request );
        return reply.GetIResult();
    }
    static async Task<IResult> DisableTwoFactor( HttpContext http, AccountSecurityManager manager )
    {
        var reply = await manager.Disable2Fa( http.UserId() );
        return reply.GetIResult();
    }
    static async Task<IResult> UpdateTwoFactor( Update2FaRequest request, HttpContext http, AccountSecurityManager manager )
    {
        var reply = await manager.Update2Fa( http.UserId(), request );
        return reply.GetIResult();
    }
}