using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Users.Registration.Systems;
using OrderingApplication.Features.Users.Registration.Types;

namespace OrderingApplication.Features.Users.Registration;

internal static class AccountRegistrationEndpoints
{
    internal static void MapRegistrationEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapPut( "api/account/register/resendConfirmEmail",
            static async ( [FromBody] string email, AccountConfirmationSystem system ) =>
            await SendEmailConfirmationLink( email, system ) );

        app.MapPut( "api/account/register/confirmEmail",
            static async ( [FromBody] ConfirmAccountEmailRequest request, AccountConfirmationSystem system ) =>
            await ConfirmEmail( request, system ) );

        app.MapPut( "api/account/register",
            static async ( [FromBody] RegisterAccountRequest request, AccountRegistrationSystem system ) =>
            await RegisterAccount( request, system ) );
    }

    static async Task<IResult> SendEmailConfirmationLink( string email, AccountConfirmationSystem system )
    {
        var reply = await system.SendEmailConfirmationLink( email );
        return reply.GetIResult();
    }
    static async Task<IResult> ConfirmEmail( ConfirmAccountEmailRequest request, AccountConfirmationSystem system )
    {
        var reply = await system.ConfirmEmail( request );
        return reply.GetIResult();
    }
    static async Task<IResult> RegisterAccount( RegisterAccountRequest request, AccountRegistrationSystem system )
    {
        var reply = await system.RegisterAccount( request );
        return reply.GetIResult();
    }
}