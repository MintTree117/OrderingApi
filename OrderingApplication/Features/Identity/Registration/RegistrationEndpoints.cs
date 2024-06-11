using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Identity.AccountDetails;
using OrderingApplication.Features.Identity.Registration.Systems;
using OrderingApplication.Features.Identity.Registration.Types;
using OrderingDomain.Optionals;

namespace OrderingApplication.Features.Identity.Registration;

internal static class RegistrationEndpoints
{
    internal static void MapRegistrationEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapPut( "api/register/email/resend", static async ( [FromBody] string email, EmailConfirmationSystem system ) =>
            (await system.SendConfirmationLink( email ))
            .GetIResult() );
        app.MapPut( "api/register/email/confirm", static async ( [FromBody] ConfirmEmailRequest request, EmailConfirmationSystem system ) =>
            (await system.ConfirmEmail( request ))
            .GetIResult() );
        app.MapPost( "api/register", static async ( [FromBody] RegisterRequest request, RegistrationSystem system ) =>
            (await system.RegisterIdentity( request ))
            .GetIResult() );
        app.MapDelete( "api/account/delete", static async ( [FromQuery] string password, HttpContext http, AccountDetailsManager system ) => 
            await DeleteAccount( password, http, system ) ).RequireAuthorization();
    }
    static async Task<IResult> DeleteAccount( string password, HttpContext http, AccountDetailsManager system )
    {
        await http.SignOutAsync( CookieAuthenticationDefaults.AuthenticationScheme );
        Reply<bool> deleteReply = await system.DeleteIdentity( http.UserId(), password );
        if (deleteReply.IsSuccess)
            await http.SignOutAsync( CookieAuthenticationDefaults.AuthenticationScheme );
        return deleteReply.GetIResult();
    }
}