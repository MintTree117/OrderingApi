using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Features.Identity.Types.Accounts;
using OrderingApplication.Features.Identity.Types.Addresses;
using OrderingApplication.Features.Identity.Types.Login;
using OrderingApplication.Features.Identity.Types.Password;
using OrderingApplication.Features.Identity.Types.Registration;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Identity.Services;

namespace OrderingApplication.Features.Identity;

internal static class IdentityEndpoints
{
    internal static void MapIdentityEndpoints( this IEndpointRouteBuilder app )
    {
        // EMAIL CONFIRMATION
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        app.MapPost( "api/identity/email/resend", static async ( [FromBody] string email, EmailConfirmationSystem system ) =>
        (await system.SendConfirmationLink( email ))
        .GetIResult() );
        app.MapPut( "api/identity/email/confirm", static async ( [FromBody] ConfirmEmailRequest request, EmailConfirmationSystem system ) =>
        (await system.ConfirmEmail( request ))
        .GetIResult() );

        // LOGIN RECOVERY
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        app.MapPost( "api/identity/password/forgot", static async ( [FromBody] ForgotPasswordRequest request, LoginRecoverySystem system ) =>
        (await system.ForgotPassword( request ))
        .GetIResult() );
        app.MapPost( "api/identity/password/reset", static async ( [FromBody] ResetPasswordRequest request, LoginRecoverySystem system ) =>
        (await system.ResetPassword( request ))
        .GetIResult() );
        
        // LOGIN
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        app.MapPost( "api/identity/login", static async ( [FromBody] LoginRequest request, LoginSystem system ) =>
            (await system.Login( request ))
            .GetIResult() );
        app.MapPost( "api/identity/login2Fa", static async ( [FromBody] TwoFactorRequest request, LoginSystem system ) =>
            (await system.Login2Factor( request ))
            .GetIResult() );

        // LOGIN REFRESH
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        app.MapPost( "api/identity/refresh", static async ( [FromBody] LoginRefreshRequest request, LoginRefreshSystem system ) =>
            (await system.LoginRefresh( request ))
            .GetIResult() );
        app.MapPost( "api/identity/refreshFull", static async ( [FromBody] LoginRefreshRequest request, LoginRefreshSystem system ) =>
            (await system.LoginRefreshFull( request ))
            .GetIResult() );

        // LOGOUT
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        app.MapPost( "api/identity/logout", static async ( [FromBody] string refreshToken, HttpContext http, LogoutSystem system ) =>
            (await system.Logout( http.User, refreshToken ))
            .GetIResult() ).RequireAuthorization();

        // REGISTRATION
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        app.MapPost( "api/identity/register", static async ( [FromBody] RegisterRequest request, RegistrationSystem system ) =>
            (await system.RegisterIdentity( request ))
            .GetIResult() );

        // USER ADDRESSES
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        app.MapGet( "api/account/address/view", static async ( [FromQuery] int page, [FromQuery] int pageSize, HttpContext http, UserAddressSystem system ) =>
            (await system.ViewAddresses( http.UserId(), new ViewAddressesRequest( page, pageSize ) ))
            .GetIResult() ).RequireAuthorization();
        app.MapPut( "api/account/address/add", static async ( [FromBody] AddressDto request, HttpContext http, UserAddressSystem system ) =>
            (await system.AddAddress( http.UserId(), request ))
            .GetIResult() ).RequireAuthorization();
        app.MapPost( "api/account/address/update", static async ( [FromBody] AddressDto request, HttpContext http, UserAddressSystem system ) =>
            (await system.UpdateAddress( http.UserId(), request ))
            .GetIResult() ).RequireAuthorization();
        app.MapDelete( "api/account/address/delete", static async ( [FromQuery] Guid addressId, HttpContext http, UserAddressSystem system ) =>
            (await system.DeleteAddress( http.UserId(), addressId ))
            .GetIResult() ).RequireAuthorization();
        
        // USER PROFILE
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        app.MapGet( "api/account/profile/view", static async ( HttpContext http, UserProfileSystem system ) =>
            (await system.ViewIdentity( http.UserId() ))
            .GetIResult() ).RequireAuthorization();
        app.MapPost( "api/account/profile/update", static async ( [FromBody] UpdateProfileRequest request, HttpContext http, UserProfileSystem system ) =>
            (await system.UpdateAccount( http.UserId(), request ))
            .GetIResult() ).RequireAuthorization();
        app.MapPost( "api/account/profile/delete", static async ( [FromBody] DeleteProfileRequest request, HttpContext http, UserProfileSystem system ) =>
            (await system.DeleteIdentity( http.UserId(), request ))
            .GetIResult() ).RequireAuthorization();

        // USER SECURITY
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        app.MapPost( "api/account/password/update", static async ( [FromBody] UpdatePasswordRequest request, HttpContext http, UserSecuritySystem system ) =>
            (await system.UpdatePassword( http.UserId(), request ))
            .GetIResult() ).RequireAuthorization();
        app.MapPost( "api/account/2fa/update", static async ( [FromBody] bool enabledTwoFactor, HttpContext http, UserSecuritySystem system ) =>
            (await system.UpdateTwoFactor( http.UserId(), enabledTwoFactor ))
            .GetIResult() ).RequireAuthorization();
    }
}