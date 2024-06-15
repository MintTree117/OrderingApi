using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Features.Users.Utilities;
using OrderingDomain.ReplyTypes;

namespace OrderingApplication.Extentions;

internal static class UtilityExtentions
{
    internal static void LogReplyError<T>( this ILogger<T> logger, IReply reply )
    {
        if (!reply.CheckSuccess())
            logger.LogError( reply.GetMessage() );
    }
    internal static void LogIdentityResultError<T>( this ILogger<T> logger, IdentityResult identityResult )
    {
        if (!identityResult.Succeeded)
            logger.LogError( identityResult.CombineErrors() );
    }
    
    internal static string GetOrThrow( this IConfiguration configuration, string section ) =>
        configuration[section] ?? throw new Exception( $"Failed to get {section} from IConfiguration." );
    internal static Exception Exception( this IConfiguration configuration, string section ) =>
        new( $"Failed to get section {section} from IConfiguration." );

    internal static string AuthType( this HttpContext context ) =>
        context.User.FindFirstValue( ClaimTypes.AuthenticationMethod ) ?? string.Empty;
    internal static string SessionId( this HttpContext context ) =>
        context.User.FindFirstValue( ClaimTypes.Sid ) ?? string.Empty;
    internal static string UserId( this HttpContext context ) =>
        context.User.FindFirstValue( ClaimTypes.NameIdentifier ) ?? string.Empty;
    internal static string Email( this HttpContext context ) =>
        context.User.FindFirstValue( ClaimTypes.Email ) ?? string.Empty;
    internal static string Username( this HttpContext context ) =>
        context.User.FindFirstValue( ClaimTypes.Name ) ?? string.Empty;
    
    internal static string UserId( this ClaimsPrincipal claims ) =>
        claims.Claims.FirstOrDefault( static c => c.Type == ClaimTypes.NameIdentifier )?.Value ?? string.Empty;
    internal static string Email( this ClaimsPrincipal claims ) =>
        claims.Claims.FirstOrDefault( static c => c.Type == ClaimTypes.Email )?.Value ?? string.Empty;
    internal static string Username( this ClaimsPrincipal claims ) =>
        claims.Claims.FirstOrDefault( static c => c.Type == ClaimTypes.Name )?.Value ?? string.Empty;
    
    internal static IResult GetIResult<T>( this Reply<T> reply ) =>
        reply.Succeeded
            ? Results.Ok( reply.Data )
            : FromReply( reply );
    internal static IResult GetIResult<T>( this Replies<T> reply ) =>
        reply.Succeeded
            ? Results.Ok( reply.Enumerable )
            : FromReply( reply );
    internal static IResult GetIResult( this IReply reply ) =>
        reply.CheckSuccess()
            ? Results.Ok( reply.GetData() )
            : FromReply( reply );
    static IResult FromReply( IReply reply ) =>
        Results.Problem( new ProblemDetails() {
            Detail = reply.GetMessage()
        } );
}