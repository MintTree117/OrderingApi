using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Account.Security.Types;
using OrderingApplication.Utilities;

namespace OrderingApplication.Features.Account.Security;

internal static class AccountSecurityEndpoints
{
    internal static void MapAccountSecurityEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapPut( "api/account/security/view", static async ( HttpContext http, AccountSecurityManager system ) =>
            (await system.GetSecuritySettings( http.UserId() ))
            .GetIResult() ).RequireAuthorization( Consts.DefaultPolicy );
        app.MapPut( "api/account/security/updatePassword", static async ( [FromBody] UpdatePasswordRequest request, HttpContext http, AccountSecurityManager system ) =>
            (await system.UpdatePassword( http.UserId(), request ))
            .GetIResult() ).RequireAuthorization( Consts.DefaultPolicy );
        app.MapPut( "api/account/security/update2Fa", static async ( [FromBody] bool enabledTwoFactor, HttpContext http, AccountSecurityManager system ) =>
            (await system.UpdateTwoFactor( http.UserId(), enabledTwoFactor ))
            .GetIResult() ).RequireAuthorization( Consts.DefaultPolicy );
    }
}