namespace OrderingDomain.ValueTypes;

public readonly record struct Contact(
    string Name, 
    string Email, 
    string? Phone )
{
    public static Contact Empty() => 
        new( string.Empty, string.Empty, string.Empty );
}