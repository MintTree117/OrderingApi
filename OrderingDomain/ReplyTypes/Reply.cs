namespace OrderingDomain.ReplyTypes;

public readonly record struct Reply<T> : IReply
{
    public static bool operator true( Reply<T> reply ) => reply.Succeeded;
    public static bool operator false( Reply<T> reply ) => !reply.Succeeded;
    public static implicit operator bool( Reply<T> reply ) => reply.Succeeded;

    // Intentionally Unsafe: Up to programmer to keep track
    public readonly bool Succeeded;
    public T Data => _obj ?? throw new Exception( $"!!!!!!!!!!!! Fatal: Reply<{typeof( T )}>: Tried to access a null reply. !!!!!!!!!!!!" );
    public object GetData() => _obj ?? throw new Exception( $"!!!!!!!!!!!! Fatal: Reply<{typeof( T )}>: Tried to access a null reply. !!!!!!!!!!!!" );
    public string GetMessage() => _message ?? string.Empty;
    public bool CheckSuccess() => Succeeded;

    readonly T? _obj = default;
    readonly string? _message = null;
    
    public bool OutSuccess( out Reply<T> self )
    {
        self = this;
        return Succeeded;
    }
    public bool OutFailure( out Reply<T> self )
    {
        self = this;
        return !Succeeded;
    }
    
    public static Reply<T> Success( T obj ) => new( obj );
    public static Reply<T> Failure() => new();
    public static Reply<T> Failure( string msg ) => new( msg );
    public static Reply<T> Failure( IReply reply ) => new( reply.GetMessage() );
    public static Reply<T> Failure( IReply reply, string msg ) => new( $"{msg} {reply.GetMessage()}" );
    public static Reply<T> Failure( Exception ex ) => new( ex );
    public static Reply<T> Failure( Exception ex, string msg ) => new( ex, msg );
    
    public static Reply<T> NotFound() =>
        Failure( MsgNotFound );
    public static Reply<T> NotFound( string msg ) =>
        Failure( string.Join( msg, MsgNotFound ) );
    public static Reply<T> NotFound( IReply other ) =>
        Failure( string.Join( MsgNotFound, other.GetMessage() ) );
    
    public static Reply<T> UserNotFound() =>
        Failure( MsgUserNotFound );
    public static Reply<T> UserNotFound( string msg ) =>
        Failure( string.Join( msg, MsgUserNotFound ) );
    public static Reply<T> UserNotFound( IReply other ) =>
        Failure( string.Join( MsgUserNotFound, other.GetMessage() ) );

    public static Reply<T> Invalid() =>
        Failure( MsgValidationFailure );
    public static Reply<T> Invalid( string msg ) =>
        Failure( string.Join( msg, MsgValidationFailure ) );
    public static Reply<T> Invalid( IReply other ) =>
        Failure( string.Join( MsgValidationFailure, other.GetMessage() ) );

    public static Reply<T> InvalidPassword() =>
        Failure( MsgPasswordFailure );
    public static Reply<T> InvalidPassword( string msg ) =>
        Failure( string.Join( msg, MsgPasswordFailure ) );
    public static Reply<T> InvalidPassword( IReply other ) =>
        Failure( string.Join( MsgPasswordFailure, other.GetMessage() ) );

    public static Reply<T> ChangesNotSaved() =>
        Failure( MsgChangesNotSaved );
    public static Reply<T> ChangesNotSaved( string msg ) =>
        Failure( string.Join( msg, MsgChangesNotSaved ) );
    public static Reply<T> ChangesNotSaved( IReply other ) =>
        Failure( string.Join( MsgChangesNotSaved, other.GetMessage() ) );

    public static Reply<T> Conflict() =>
        Failure( MsgConflictError );
    public static Reply<T> Conflict( string msg ) =>
        Failure( string.Join( msg, MsgConflictError ) );
    public static Reply<T> Conflict( IReply other ) =>
        Failure( string.Join( MsgConflictError, other.GetMessage() ) );
    
    public static Reply<T> ServerError() =>
        Failure( MsgServerError );
    public static Reply<T> ServerError( string msg ) =>
        Failure( string.Join( msg, MsgServerError ) );
    public static Reply<T> ServerError( IReply other ) =>
        Failure( string.Join( MsgServerError, other.GetMessage() ) );

    public static Reply<T> Unauthorized() =>
        Failure( MsgUnauthorized );
    public static Reply<T> Unauthorized( string msg ) =>
        Failure( string.Join( msg, MsgUnauthorized ) );
    public static Reply<T> Unauthorized( IReply other ) =>
        Failure( string.Join( MsgUnauthorized, other.GetMessage() ) );

    const string MsgNotFound = "Request not found.";
    const string MsgUserNotFound = "User not found.";
    const string MsgValidationFailure = "Validation failed.";
    const string MsgPasswordFailure = "Invalid password.";
    const string MsgChangesNotSaved = "Failed to save changes to storage.";
    const string MsgConflictError = "A conflict has occured.";
    const string MsgServerError = "An internal server error occured.";
    const string MsgUnauthorized = "Unauthorized.";

    Reply( T obj )
    {
        _obj = obj;
        Succeeded = true;
    }
    Reply( string? message = null ) =>
        _message = message;
    Reply( Exception e, string? message = null ) => 
        _message = $"{message} : Exception : {e} : {e.Message}";
}