using Microsoft.AspNetCore.Identity;
using OrderingApplication.Extentions;
using OrderingDomain.ReplyTypes;

namespace OrderingApplication.Features;

internal abstract class BaseService<T>( ILogger<T> logger )
{
    protected readonly ILogger<T> Logger = logger;

    protected void LogReplyError( IReply reply ) =>
        Logger.LogReplyError( reply );
    protected void LogIdentityResultError( IdentityResult identityResult ) =>
        Logger.LogIdentityResultError( identityResult );
    protected void LogError( string message ) =>
        Logger.LogError( message );
    protected void LogException( Exception exception, string message ) =>
        Logger.LogError( exception, message );
    protected void LogInfoOrError( bool condition, string infoMessage, string errorMessage )
    {
        if (condition) Logger.LogInformation( infoMessage );
        else Logger.LogError( errorMessage );
    }
}