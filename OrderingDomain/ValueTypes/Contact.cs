namespace OrderingDomain.ValueTypes;

public sealed class Contact
{
    public Contact() { }

    public Contact(string name, string email, string? phone)
    {
        Name = name;
        Email = email;
        Phone = phone;
    }
    public static Contact Empty() => 
        new( string.Empty, string.Empty, string.Empty );

    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
}   