namespace OrderingDomain.ReplyTypes;

public readonly record struct Replies<T> : IReply
{
    public static bool operator true( Replies<T> reply ) => reply.Succeeded;
    public static bool operator false( Replies<T> reply ) => !reply.Succeeded;
    public static implicit operator bool( Replies<T> reply ) => reply.Succeeded;
    
    readonly IEnumerable<T>? _enumerable = null;
    readonly string? _message = string.Empty;

    public readonly bool Succeeded;
    public IEnumerable<T> Enumerable => _enumerable ?? Array.Empty<T>();
    public object GetData() => _enumerable ?? Array.Empty<T>();
    public string GetMessage() => _message ?? string.Empty;
    public bool CheckSuccess() => Succeeded;
    
    public bool OutFailure( out IReply self )
    {
        self = this;
        return !Succeeded;
    }
    public bool OutSuccess( out Replies<T> self )
    {
        self = this;
        return Succeeded;
    }
    public bool OutFailure( out Replies<T> self )
    {
        self = this;
        return !Succeeded;
    }
    
    public static Replies<T> Success( IEnumerable<T> objs ) => new( objs );
    public static Replies<T> Fail() => new();
    public static Replies<T> Fail( string msg ) => new( msg );
    public static Replies<T> Fail( IReply reply ) => new( reply.GetMessage() );
    public static Replies<T> Fail( Exception ex ) => new( ex );
    public static Replies<T> Fail( Exception ex, string msg ) => new( ex, msg );

    Replies( IEnumerable<T>? enumerable )
    {
        _enumerable = enumerable;
        Succeeded = true;
    }
    Replies( string? message = null ) => _message = message;
    Replies( Exception e, string? message = null ) => _message = $"{message} : Exception : {e} : {e.Message}";
}