namespace OrderingDomain.ReplyTypes;

public readonly record struct ReplyLine<T>(
    List<Reply<T>> Options ) where T : class, new()
{
    public bool AsyncOut( out ReplyLine<T> self )
    {
        self = this;
        return !AnyFailedOut( out self );
    }
    public bool AnySucceeded => Options.Any( static o => o.Succeeded );
    public bool AnyFailedOut( out ReplyLine<T> self )
    {
        self = this;
        return Options.Any( static o => !o.Succeeded );
    }
    public List<T> ToObjects() => Options.Select( static o => o.Data ).ToList();
}