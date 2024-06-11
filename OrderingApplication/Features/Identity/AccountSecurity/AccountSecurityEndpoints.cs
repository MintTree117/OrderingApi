using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Identity.AccountSecurity.Types;
using OrderingApplication.Features.Identity.Login.Services;

namespace OrderingApplication.Features.Identity.AccountSecurity;

internal static class AccountSecurityEndpoints
{
    internal static void MapAccountSecurityEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapPut( "api/identity/password/forgot", static async ( [FromBody] ForgotPasswordRequest request, LoginRecoverySystem system ) =>
        (await system.ForgotPassword( request ))
        .GetIResult() );
        app.MapPut( "api/identity/password/reset", static async ( [FromBody] ResetPasswordRequest request, LoginRecoverySystem system ) =>
        (await system.ResetPassword( request ))
        .GetIResult() );
        app.MapPost( "api/account/password/update", static async ( [FromBody] UpdatePasswordRequest request, HttpContext http, AccountSecurityManager system ) =>
            (await system.UpdatePassword( http.UserId(), request ))
            .GetIResult() ).RequireAuthorization();
        app.MapPost( "api/account/2fa/update", static async ( [FromBody] bool enabledTwoFactor, HttpContext http, AccountSecurityManager system ) =>
            (await system.UpdateTwoFactor( http.UserId(), enabledTwoFactor ))
            .GetIResult() ).RequireAuthorization();
    }
}