using OrderingDomain.ValueTypes;

namespace OrderingDomain.Users;

public sealed class UserAddress
{
    public UserAddress() { }
    public UserAddress( Guid id, string userId, bool isPrimary, string name, Address address )
    {
        Id = id;
        UserId = userId;
        IsPrimary = isPrimary;
        Name = name;
        PosX = address.PosX;
        PosY = address.PosY;
    }
    public UserAddress( Guid id, string userId, bool isPrimary, string name, int posX, int posY )
    {
        Id = id;
        UserId = userId;
        IsPrimary = isPrimary;
        Name = name;
        PosX = posX;
        PosY = posY;
    }

    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public string Name { get; set; } = string.Empty;
    public int PosX { get; set; }
    public int PosY { get; set; }
}