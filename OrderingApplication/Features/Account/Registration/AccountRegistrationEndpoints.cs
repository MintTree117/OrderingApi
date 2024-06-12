using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Account.Registration.Systems;
using OrderingApplication.Features.Account.Registration.Types;

namespace OrderingApplication.Features.Account.Registration;

internal static class AccountRegistrationEndpoints
{
    internal static void MapRegistrationEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapPut( "api/account/register/confirmEmail/resend", static async ( [FromBody] string email, AccountConfirmationSystem system ) =>
            (await system.SendEmailConfirmationLink( email ))
            .GetIResult() );
        app.MapPut( "api/account/register/confirmEmail", static async ( [FromBody] ConfirmAccountEmailRequest request, AccountConfirmationSystem system ) =>
            (await system.ConfirmEmail( request ))
            .GetIResult() );
        app.MapPost( "api/account/register", static async ( [FromBody] RegisterAccountRequest request, Systems.AccountRegisterSystem system ) =>
            (await system.RegisterIdentity( request ))
            .GetIResult() );
    }
}