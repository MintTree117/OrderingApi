using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Account.Login.Services;
using OrderingApplication.Features.Account.Login.Types;
using OrderingApplication.Features.Account.Utilities;
using OrderingDomain.Optionals;

namespace OrderingApplication.Features.Account.Login;

internal static class LoginEndpoints
{
    const string Cookies = "Cookies";
    
    internal static void MapLoginEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapPost( "api/account/login", static async ( [FromBody] LoginRequest request, HttpContext http, IdentityConfigCache config, LoginSystem system ) =>
            await Login( request, http, config, system ) );
        
        app.MapPost( "api/account/login2Fa", static async ( [FromBody] TwoFactorRequest request, HttpContext http, IdentityConfigCache config, Login2FaSystem system ) =>
            await Login2Fa( request, http, config, system ) );

        app.MapPost( "api/account/loginRecovery", static async ( [FromBody] TwoFactorRequest request, HttpContext http, IdentityConfigCache config, Login2FaSystem system ) =>
            await Login2Fa( request, http, config, system ) );
        
        app.MapGet( "api/account/loginRefresh", static async ( HttpContext http, LoginRefreshSystem refresher ) =>
            await refresher.GetAccessToken( http.UserId() ) )
            .RequireAuthorization( Cookies );
        
        app.MapPut( "api/account/logout", static async ( [FromBody] string refreshToken, HttpContext http ) =>
            await http.SignOutAsync( CookieAuthenticationDefaults.AuthenticationScheme ) )
            .RequireAuthorization( Cookies );
    }

    static async Task<IResult> Login( LoginRequest request, HttpContext http, IdentityConfigCache config, LoginSystem system )
    {
        Reply<LoginResponse> jwtResult = await system.Login( request );
        if (!jwtResult.IsSuccess || jwtResult.Data.IsPending2Fa)
            return jwtResult.GetIResult();

        return await SignInHttpCookies( jwtResult.Data.AccessToken ?? string.Empty, config.JwtConfigRules, http, jwtResult.Data );
    }
    static async Task<IResult> Login2Fa( TwoFactorRequest request, HttpContext http, IdentityConfigCache config, Login2FaSystem system )
    {
        Reply<TwoFactorResponse> twoFaReply = await system.Login2Factor( request );
        if (!twoFaReply.IsSuccess)
            return twoFaReply.GetIResult();

        return await SignInHttpCookies( twoFaReply.Data.AccessToken, config.JwtConfigRules, http, twoFaReply.Data );
    }
    static async Task<IResult> SignInHttpCookies( string accessToken, JwtConfig config, HttpContext http, object data )
    {
        Reply<bool> parseReply = JwtUtils.ParseToken( accessToken, config, out ClaimsPrincipal? claims, out JwtSecurityToken? jwt );
        if (!parseReply.IsSuccess)
            return Results.Problem( "An error occured while signing in with cookies." );

        await http.SignInAsync( claims! );
        return Results.Ok( data );
    }
}