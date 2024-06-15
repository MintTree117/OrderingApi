using OrderingDomain.ValueTypes;

namespace OrderingApplication.Features.Ordering.Dtos;

public static class AddressExtentions
{
    public static HeuristicDistance HeuristicDistanceFrom( this WorldGridPos worldGridPos, WorldGridPos other )
    {
        return new HeuristicDistance();
    }
}