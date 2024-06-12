using OrderingDomain._Common;
using OrderingDomain.ValueTypes;

namespace OrderingDomain.Account;

public sealed class UserAddress : IEntity
{
    public UserAddress() { }
    public UserAddress( Guid id, string userId, bool isPrimary, Address address )
    {
        Id = id;
        UserId = userId;
        IsPrimary = isPrimary;
        Address = address;
    }
    public UserAddress( Guid id, string userId, bool isPrimary, int posX, int posY )
    {
        Id = id;
        UserId = userId;
        IsPrimary = isPrimary;
        Address = new Address( posX, posY );
    }

    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public Address Address { get; set; }
}