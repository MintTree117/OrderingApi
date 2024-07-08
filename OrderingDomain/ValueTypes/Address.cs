namespace OrderingDomain.ValueTypes;

public readonly record struct Address(
    string Name,
    int PosX,
    int PosY );