using OrderingDomain.Cart;
using OrderingDomain.ReplyTypes;

namespace OrderingApplication.Features.Cart;

internal static class CartExtentions
{
    internal static Reply<List<CartItemDto>> Dtos( this List<Reply<CartItem>> models )
    {
        List<CartItemDto> dtos = [];
        dtos.AddRange( from m in models select MapToDto( m.Data ) );
        return Reply<List<CartItemDto>>.Success( dtos );
    }
    internal static List<CartItem> Models( this IEnumerable<CartItemDto> dtos )
    {
        List<CartItem> models = [];
        models.AddRange( from d in dtos select MapToModel( d ) );
        return models;
    }

    static CartItemDto MapToDto( CartItem item )
        => CartItemDto.New( item.ProductId, item.Quantity );
    static CartItem MapToModel( CartItemDto dto )
        => CartItem.WithoutUserId( dto.ProductId, dto.Quantity );
}