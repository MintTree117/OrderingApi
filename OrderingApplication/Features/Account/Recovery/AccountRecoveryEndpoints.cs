using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Account.Recovery.Systems;
using OrderingApplication.Features.Account.Recovery.Types;

namespace OrderingApplication.Features.Account.Recovery;

internal static class AccountRecoveryEndpoints
{
    internal static void MapAccountRecoveryEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapPut( "api/identity/password/forgot", static async ( [FromBody] ForgotPasswordRequest request, PasswordRecoverySystem system ) =>
        (await system.ForgotPassword( request ))
        .GetIResult() );
        app.MapPut( "api/identity/password/reset", static async ( [FromBody] ResetPasswordRequest request, PasswordRecoverySystem system ) =>
        (await system.ResetPassword( request ))
        .GetIResult() );
    }
}