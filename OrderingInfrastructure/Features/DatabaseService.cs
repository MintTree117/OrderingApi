using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderingDomain.ReplyTypes;

namespace OrderingInfrastructure.Features;

internal abstract class DatabaseService<TService>( DbContext context, ILogger<TService> logger )
{
    protected const string MsgDbNoChanges = "No changes were recorded in the database.";
    protected const string MsgDbException = "An exception occured while accessing the database.";
    
    protected readonly ILogger<TService> Logger = logger;
    readonly DbContext _context = context;
    
    public async Task<Reply<bool>> SaveAsync()
    {
        try
        {
            return await _context.SaveChangesAsync() > 0
                ? Reply<bool>.Success( true )
                : Reply<bool>.Failure( MsgDbNoChanges );
        }
        catch ( Exception e )
        {
            return ProcessDbException<bool>( e );
        }
    }
    protected Reply<T> ProcessDbException<T>( Exception e )
    {
        Logger.LogError( e, e.Message );
        return Reply<T>.Failure( MsgDbException );
    }
    protected Replies<T> ProcessDbExceptionReplies<T>( Exception e )
    {
        Logger.LogError( e, e.Message );
        return Replies<T>.Fail( MsgDbException );
    }
}