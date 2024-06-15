using OrderingDomain.ValueTypes;

namespace OrderingDomain.Users;

public sealed class UserAddress
{
    public UserAddress() { }
    public UserAddress( Guid id, string userId, bool isPrimary, string name, WorldGridPos worldGridPos )
    {
        Id = id;
        UserId = userId;
        IsPrimary = isPrimary;
        Name = name;
        WorldGridPosX = worldGridPos.GridX;
        WorldGridPosY = worldGridPos.GridY;
    }
    public UserAddress( Guid id, string userId, bool isPrimary, string name, int posX, int posY )
    {
        Id = id;
        UserId = userId;
        IsPrimary = isPrimary;
        Name = name;
        WorldGridPosX = posX;
        WorldGridPosY = posY;
    }

    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public string Name { get; set; } = string.Empty;
    public int WorldGridPosX { get; set; }
    public int WorldGridPosY { get; set; }
}