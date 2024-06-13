using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Users.Authentication.Services;
using OrderingApplication.Features.Users.Authentication.Types;
using OrderingDomain.ReplyTypes;

namespace OrderingApplication.Features.Users.Authentication;

internal static class AuthenticationEndpoints
{
    const string Cookies = "Cookies";
    
    internal static void MapAuthenticationEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapPost( "api/authenticate/login",
            static async ( [FromBody] LoginRequest request, HttpContext http, LoginManager login, SessionManager sessions ) =>
            await Login( request, http, login, sessions ) );
        
        app.MapPost( "api/authenticate/2fa", 
            static async ( [FromBody] TwoFactorRequest request, HttpContext http, LoginManager system, SessionManager sessions ) =>
            await Login2Fa( request, http, system, sessions ) );

        app.MapPost( "api/authenticate/recover", 
            static async ( [FromBody] LoginRecoveryRequest request, LoginManager manager ) =>
            await LoginRecovery( request, manager ) );

        app.MapPost( "api/authenticate/refresh",
            static async ( HttpContext http, SessionManager sessions ) =>
                await SessionRefresh( http, sessions ) )
            .RequireAuthorization( Cookies );

        app.MapPut( "api/authenticate/forgot", 
            static async ( [FromBody] string email, PasswordResetter resetter ) =>
            await SendResetEmail( email, resetter ) );

        app.MapPut( "api/authenticate/reset",
            static async ( [FromBody] ResetPasswordRequest request, PasswordResetter resetter ) =>
            await ResetPassword( request, resetter ) );

        app.MapPut( "api/authenticate/logout",
            static async ( HttpContext http, SessionManager sessions ) =>
                await SessionRevoke( http, sessions ) )
            .RequireAuthorization( Cookies );
    }
    
    static async Task<IResult> Login( LoginRequest request, HttpContext http, LoginManager manager, SessionManager sessions )
    {
        var loginReply = await manager.Login( request );
        if (!loginReply || loginReply.Data.IsPending2Fa)
            return loginReply.GetIResult();
        
        return await HandleLoginReply( loginReply, http, sessions );
    }
    static async Task<IResult> Login2Fa( TwoFactorRequest request, HttpContext http, LoginManager manager, SessionManager sessions )
    {
        var loginReply = await manager.Login2Factor( request );
        if (!loginReply)
            return loginReply.GetIResult();
        
        return await HandleLoginReply( loginReply, http, sessions );
    }
    static async Task<IResult> LoginRecovery( LoginRecoveryRequest request, LoginManager manager )
    {
        var loginReply = await manager.LoginRecovery( request );
        return loginReply.GetIResult(); //DO NOT SIGN IN COOKIES BECAUSE THIS IS SHORT-LIVED RECOVERY ACCESS
    }
    
    static async Task<IResult> SendResetEmail( string email, PasswordResetter manager )
    {
        var emailReply = await manager.ForgotPassword( email );
        return emailReply.GetIResult();
    }
    static async Task<IResult> ResetPassword( [FromBody] ResetPasswordRequest request, PasswordResetter manager )
    {
        var emailReply = await manager.ResetPassword( request );
        return emailReply.GetIResult();
    }

    static async Task<IResult> SessionRefresh( HttpContext http, SessionManager manager )
    {
        var refreshReply = await manager.GetRefreshedToken( http.Session.Id, http.UserId() );

        if (refreshReply)
            return Results.Ok( refreshReply.Data );

        await http.SignOutAsync();
        return Results.Problem( refreshReply.GetMessage() );
    }
    static async Task<IResult> SessionRevoke( HttpContext http, SessionManager manager )
    {
        var sessionReply = await manager.RevokeSession( http.Session.Id, http.UserId() );
        await http.SignOutAsync( CookieAuthenticationDefaults.AuthenticationScheme );
        return sessionReply.GetIResult();
    }

    static async Task<IResult> HandleLoginReply( Reply<LoginInfo> loginReply, HttpContext http, SessionManager sessions )
    {
        await http.SignInAsync( loginReply.Data.ClaimsPrincipal! );
        await sessions.AddSession( http.Session.Id, http.UserId() );

        var loginResponse = LoginResponse.LoggedIn( loginReply.Data.AccessToken! );
        return Results.Ok( loginResponse );
    }
}