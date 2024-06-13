using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Users.Security.Types;
using OrderingApplication.Utilities;

namespace OrderingApplication.Features.Users.Security;

internal static class AccountSecurityEndpoints
{
    internal static void MapAccountSecurityEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapGet( "api/account/security/view", static async ( HttpContext http, AccountSecurityManager system ) =>
            (await system.GetSecuritySettings( http.UserId() ))
            .GetIResult() ).RequireAuthorization( Consts.DefaultPolicy );
        app.MapPut( "api/account/security/updatePassword", static async ( [FromBody] UpdatePasswordRequest request, HttpContext http, AccountSecurityManager system ) =>
            (await system.UpdatePassword( http.UserId(), request ))
            .GetIResult() ).RequireAuthorization( Consts.DefaultPolicy );
        app.MapPut( "api/account/security/update2Fa", static async ( [FromBody] Update2FaRequest request, HttpContext http, AccountSecurityManager system ) =>
            (await system.Update2Fa( http.UserId(), request ))
            .GetIResult() ).RequireAuthorization( Consts.DefaultPolicy );
    }
}