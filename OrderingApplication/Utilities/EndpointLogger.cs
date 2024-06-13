using Microsoft.AspNetCore.Identity;
using OrderingApplication.Extentions;
using OrderingDomain.ReplyTypes;

namespace OrderingApplication.Utilities;

internal sealed class EndpointLogger // Static Singleton
{
    internal static ILogger<EndpointLogger> Logger { get; set; } = null!;

    internal static void LogReplyError( IReply reply ) =>
        Logger.LogReplyError( reply );
    internal static void LogIdentityResultError( IdentityResult identityResult ) =>
        Logger.LogIdentityResultError( identityResult );
    internal static void LogInformation( string message ) =>
        Logger.LogInformation( message );
    internal static void LogError( string message ) =>
        Logger.LogError( message );
    internal static void LogException( Exception exception, string message ) =>
        Logger.LogError( exception, message );

    internal static void EndpointHit( string endpoint, Dictionary<string, object>? vars = null ) =>
        Logger.LogInformation( $"Endpoint Hit: {endpoint} {GetVars( vars )}" );
    internal static void EndpointSuccess( string endpoint, Dictionary<string, object>? vars = null ) =>
        Logger.LogInformation( $"Endpoint Success: {endpoint} {GetVars( vars )}" );
    internal static void EndpointFail( string endpoint, Dictionary<string, object>? vars = null ) =>
        Logger.LogInformation( $"Endpoint Fail: {endpoint} {GetVars( vars )}" );

    internal static void EndpointResult( string endpoint, IReply reply, Dictionary<string, object>? vars = null )
    {
        if (reply.CheckSuccess()) EndpointSuccess( endpoint, vars );
        else EndpointFail( endpoint, vars );
    }
    static string GetVars( Dictionary<string, object>? vars )
    {
        if (vars is null)
            return string.Empty;
        
        string str = string.Empty;
        foreach ( var kvp in vars )
            str = $"{str}, {kvp.Key} = {kvp.Value} ";
        
        return str;
    }
}