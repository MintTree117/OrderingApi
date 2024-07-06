using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Utilities;
using OrderingInfrastructure.Features.Cart;

namespace OrderingApplication.Features.Cart;

internal static class CartEndpoints
{
    internal static void MapCartEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapPost( "api/cart/postGet",
                static async ( [FromBody] List<CartItemDto> items, HttpContext http, ICartRepository repository ) =>
                await PostGet( items, http, repository ) )
            .RequireAuthorization( Consts.DefaultPolicy );

        app.MapDelete( "api/cart/clear",
                static async ( HttpContext http, ICartRepository repository ) =>
                await Clear( http, repository ) )
            .RequireAuthorization( Consts.DefaultPolicy );
    }

    static async Task<IResult> PostGet( List<CartItemDto> items, HttpContext http, ICartRepository repository )
    {
        var reply = await repository.PostGet( http.UserId(), items.Models() );
        return reply.GetIResult();
    }
    static async Task<IResult> Clear( HttpContext http, ICartRepository repository )
    {
        var reply = await repository.Clear( http.UserId() );
        return reply.GetIResult();
    }
}