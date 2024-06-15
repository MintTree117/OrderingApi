using Microsoft.AspNetCore.Identity;
using OrderingApplication.Extentions;
using OrderingDomain.ReplyTypes;

namespace OrderingApplication.Features;

internal abstract class BaseService<T>( ILogger<T> logger )
{
    protected readonly ILogger<T> Logger = logger;

    protected void LogIfErrorReply( IReply reply ) =>
        Logger.LogReplyError( reply );
    protected void LogIfErrorResult( IdentityResult identityResult ) =>
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

    protected Reply<Tlog> LogIfErrorReplyReturn<Tlog>( Reply<Tlog> reply )
    {
        LogIfErrorReply( reply );
        return reply;
    }
    protected IReply LogIfErrorReplyReturn( IReply reply )
    {
        LogIfErrorReply( reply );
        return reply;
    }
}