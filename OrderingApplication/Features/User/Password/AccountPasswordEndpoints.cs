using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.User.Password.Types;

namespace OrderingApplication.Features.User.Password;

internal static class AccountPasswordEndpoints
{
    internal static void MapAccountPasswordEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapPut( "api/account/forgotPassword", static async ( [FromBody] ForgotPasswordRequest request, AccountPasswordSystem system ) =>
        (await system.ForgotPassword( request ))
        .GetIResult() );
        app.MapPut( "api/account/resetPassword", static async ( [FromBody] ResetPasswordRequest request, AccountPasswordSystem system ) =>
        (await system.ResetPassword( request ))
        .GetIResult() );
    }
}