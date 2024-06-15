using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Users.Authentication.Services;
using OrderingApplication.Features.Users.Authentication.Types;
using OrderingApplication.Utilities;
using OrderingDomain.ReplyTypes;

namespace OrderingApplication.Features.Users.Authentication;

internal static class AuthenticationEndpoints
{
    internal static void MapAuthenticationEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapPost( "api/authentication/login",
            static async ( [FromBody] LoginRequest request, HttpContext http, LoginManager login, SessionManager sessions ) =>
            await Login( request, http, login, sessions ) );
        
        app.MapPost( "api/authentication/2fa", 
            static async ( [FromBody] TwoFactorRequest request, HttpContext http, LoginManager system, SessionManager sessions ) =>
            await Login2Fa( request, http, system, sessions ) );

        app.MapPost( "api/authentication/recover", 
            static async ( [FromBody] LoginRecoveryRequest request, LoginManager manager ) =>
            await LoginRecovery( request, manager ) );

        app.MapPost( "api/authentication/refresh",
            static async ( HttpContext http, SessionManager sessions ) =>
                await RefreshSession( http, sessions ) ).RequireAuthorization( Consts.CookiePolicy );

        app.MapPut( "api/authentication/forgot", 
            static async ( [FromBody] string email, PasswordResetter resetter ) =>
            await SendResetEmail( email, resetter ) );

        app.MapPut( "api/authentication/reset",
            static async ( [FromBody] ResetPasswordRequest request, PasswordResetter resetter ) =>
            await ResetPassword( request, resetter ) );

        app.MapPut( "api/authentication/logout",
            static async ( HttpContext http, SessionManager sessions ) =>
                await SessionRevoke( http, sessions ) ).RequireAuthorization( Consts.CookiePolicy ); // tokens arent persisted
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
    static async Task<IResult> RefreshSession( HttpContext http, SessionManager manager )
    {
        // TODO: Just for debugging purposes
        if (http.AuthType() != Consts.CookiePolicy)
        {
            EndpointLogger.LogInformation( "Refresh endpoint is accessed but not cookies." );
            return Results.Unauthorized();
        }

        var refreshReply = await manager.UpdateSession( http.SessionId(), http.UserId() );
        if (!refreshReply.Succeeded)
            await http.SignOutAsync();
        EndpointLogger.LogInformation( refreshReply.GetMessage() );
        return refreshReply.GetIResult();
    }
    static async Task<IResult> SessionRevoke( HttpContext http, SessionManager manager )
    {
        var sessionReply = await manager.RevokeSession( http.SessionId(), http.UserId() );
        await http.SignOutAsync( CookieAuthenticationDefaults.AuthenticationScheme );
        return sessionReply.GetIResult();
    }
    static async Task<IResult> HandleLoginReply( Reply<LoginInfo> loginReply, HttpContext http, SessionManager sessions )
    {
        string httpSessionId = Guid.NewGuid().ToString();
        
        List<Claim> httpOnlyClaims = [];
        httpOnlyClaims.AddRange( loginReply.Data.ClaimsPrincipal?.Claims ?? [] );
        httpOnlyClaims.Add( new Claim( ClaimTypes.Sid, httpSessionId ) );
        httpOnlyClaims.Add( new Claim( ClaimTypes.AuthenticationMethod, Consts.CookiePolicy ) );
        ClaimsPrincipal httpOnlyClaimsPrincipal = new( new ClaimsIdentity( httpOnlyClaims ) );
        
        // HTTP CONTEXT IS NOT UPDATED RIGHT AWAY HERE, MAKE SURE TO USE CLAIMS FROM LOGIN REPLY !!! NOT !!! FROM HTTP CONTEXT
        await http.SignInAsync( httpOnlyClaimsPrincipal, new AuthenticationProperties { IsPersistent = true } );
        await sessions.AddSession( httpSessionId, loginReply.Data.ClaimsPrincipal!.UserId() );

        var loginResponse = LoginResponse.LoggedIn( loginReply.Data.AccessToken! );
        return Results.Ok( loginResponse );
    }
}