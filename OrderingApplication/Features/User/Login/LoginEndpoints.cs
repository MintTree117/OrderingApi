using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.User.Login.Services;
using OrderingApplication.Features.User.Login.Types;
using OrderingApplication.Features.User.Utilities;
using OrderingDomain.ReplyTypes;

namespace OrderingApplication.Features.User.Login;

internal static class LoginEndpoints
{
    const string Cookies = "Cookies";
    
    internal static void MapLoginEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapPost( "api/account/login", static async ( [FromBody] LoginRequest request, HttpContext http, AccountConfig config, LoginSystem system ) =>
            await Login( request, http, config, system ) );
        
        app.MapPost( "api/account/login2Fa", static async ( [FromBody] TwoFactorRequest request, HttpContext http, AccountConfig config, Login2FaSystem system ) =>
            await Login2Fa( request, http, config, system ) );

        app.MapPost( "api/account/loginRecovery", static async ( [FromBody] LoginRecoveryRequest request, LoginRecoverySystem system ) =>
            await LoginRecovery( request, system ) );

        app.MapGet( "api/account/loginRefresh", static async ( HttpContext http, LoginRefreshSystem system ) =>
            await LoginRefresh( http, system ) )
            .RequireAuthorization( Cookies );

        app.MapPut( "api/account/logout", static async ( HttpContext http, LogoutSystem system ) =>
            await Logout( http, system ) )
            .RequireAuthorization( Cookies );
    }
    
    static async Task<IResult> Login( LoginRequest request, HttpContext http, AccountConfig config, LoginSystem system )
    {
        Reply<LoginResponse> jwtResult = await system.Login( request );
        if (!jwtResult.Succeeded || jwtResult.Data.IsPending2Fa)
            return jwtResult.GetIResult();

        return await SignInHttpCookies( jwtResult.Data.AccessToken ?? string.Empty, config.JwtConfigRules, http, jwtResult.Data );
    }
    static async Task<IResult> Login2Fa( TwoFactorRequest request, HttpContext http, AccountConfig config, Login2FaSystem system )
    {
        Reply<TwoFactorResponse> twoFaReply = await system.Login2Factor( request );
        if (!twoFaReply.Succeeded)
            return twoFaReply.GetIResult();

        return await SignInHttpCookies( twoFaReply.Data.AccessToken, config.JwtConfigRules, http, twoFaReply.Data );
    }
    static async Task<IResult> LoginRecovery( LoginRecoveryRequest request, LoginRecoverySystem system )
    {
        Reply<LoginResponse> loginReply = await system.LoginRecovery( request );
        return loginReply.GetIResult(); //DO NOT SIGN IN COOKIES BECAUSE THIS IS SHORT-LIVED RECOVERY ACCESS
    }
    static async Task<IResult> LoginRefresh( HttpContext http, LoginRefreshSystem system )
    {
        Reply<string> tokenReply = await system.GetAccessToken( http.UserId() );
        return tokenReply.GetIResult();
    }
    static async Task<IResult> Logout( HttpContext http, LogoutSystem system )
    {
        await http.SignOutAsync( CookieAuthenticationDefaults.AuthenticationScheme );
        Reply<bool> sessionReply = await system.Logout( http.UserId(), http.Session );
        return sessionReply.GetIResult();
    }
    static async Task<IResult> SignInHttpCookies( string accessToken, JwtConfig config, HttpContext http, object data )
    {
        Reply<bool> parseReply = JwtUtils.ParseToken( accessToken, config, out ClaimsPrincipal? claims, out JwtSecurityToken? jwt );
        if (!parseReply.Succeeded)
            return Results.Problem( "An error occured while signing in with cookies." );

        await http.SignInAsync( claims! );
        return Results.Ok( data );
    }
}