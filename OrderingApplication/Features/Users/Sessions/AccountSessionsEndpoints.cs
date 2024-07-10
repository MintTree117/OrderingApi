using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Users.Authentication.Services;
using OrderingApplication.Utilities;

namespace OrderingApplication.Features.Users.Sessions;

internal static class AccountSessionsEndpoints
{
    internal static void MapAccountSessionsEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapGet( "api/account/sessions/view",
                static async ( [FromQuery] int page, [FromQuery] int pageSize, HttpContext http, SessionManager sessions ) =>
                await View( page, pageSize, http, sessions ) )
            .RequireAuthorization( Consts.DefaultPolicy );

        app.MapDelete( "api/account/sessions/delete",
                static async ( [FromQuery] string sessionId, HttpContext http, SessionManager sessions ) =>
                await Delete( sessionId, http, sessions ) )
            .RequireAuthorization( Consts.DefaultPolicy );
    }

    static async Task<IResult> View( int page, int pageSize, HttpContext http, SessionManager service )
    {
        var reply = await service.ViewUserSessions( http.UserId(), page, pageSize );
        return reply
            ? Results.Ok( reply.Data )
            : Results.Problem( reply.GetMessage() );
    }
    static async Task<IResult> Delete( string sessionId, HttpContext http, SessionManager service )
    {
        var reply = await service.RevokeSession( sessionId, http.UserId() );
        if (!reply)
            return Results.Problem( reply.GetMessage() );

        if (sessionId != http.SessionId()) 
            return Results.Ok( false );
        
        await http.SignOutAsync();
        return Results.Ok( true );
    }
}