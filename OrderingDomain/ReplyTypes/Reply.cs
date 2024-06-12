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
    
    public static Reply<T> UserNotFound() =>
        Failure( MsgUserNotFound );
    public static Reply<T> UserNotFound( string msg ) =>
        Failure( string.Join( msg, MsgUserNotFound ) );

    public static Reply<T> Invalid() =>
        Failure( MsgValidationFailure );
    public static Reply<T> Invalid( string msg ) =>
        Failure( string.Join( msg, MsgValidationFailure ) );

    public static Reply<T> ChangesNotSaved() =>
        Failure( MsgChangesNotSaved );
    public static Reply<T> ChangesNotSaved( string msg ) =>
        Failure( string.Join( msg, MsgChangesNotSaved ) );

    public const string MsgUserNotFound = "User not found.";
    public const string MsgValidationFailure = "Validation Failed.";
    public const string MsgChangesNotSaved = "Failed to save changes to storage.";

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