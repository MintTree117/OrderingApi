namespace OrderingDomain.ValueTypes;

public sealed class Address
{
    public Address() { }

    public Address( string name, int posX, int posY )
    {
        Name = name;
        PosX = posX;
        PosY = posY;
    }
    
    public string Name { get; init; } = string.Empty;
    public int PosX { get; init; }
    public int PosY { get; init; }
}