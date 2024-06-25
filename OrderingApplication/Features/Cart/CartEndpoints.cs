using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingInfrastructure.Features.Cart;

namespace OrderingApplication.Features.Cart;

internal static class CartEndpoints
{
    internal static void MapCartEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapPost( "api/cart/postGet",
                static async ( [FromBody] List<CartItemDto> items, HttpContext http, ICartRepository repository ) =>
                await PostGet( items, http, repository ) )
            .RequireAuthorization();

        app.MapPut( "api/cart/add",
                static async ( [FromBody] Guid productId, HttpContext http, ICartRepository repository ) =>
                await Add( productId, http, repository ) )
            .RequireAuthorization();

        app.MapPut( "api/cart/update",
                static async ( [FromBody] CartItemDto item, HttpContext http, ICartRepository repository ) =>
                await Update( item, http, repository ) )
            .RequireAuthorization();

        app.MapDelete( "api/cart/delete",
                static async ( [FromQuery] Guid productId, HttpContext http, ICartRepository repository ) =>
                await Delete( productId, http, repository ) )
            .RequireAuthorization();

        app.MapDelete( "api/cart/clear",
                static async ( HttpContext http, ICartRepository repository ) =>
                await Clear( http, repository ) )
            .RequireAuthorization();
    }

    static async Task<IResult> PostGet( List<CartItemDto> items, HttpContext http, ICartRepository repository )
    {
        var reply = await repository.PostGet( http.UserId(), items.Models() );
        return reply.GetIResult();
    }
    static async Task<IResult> Add( Guid productId, HttpContext http, ICartRepository repository )
    {
        var reply = await repository.Add( http.UserId(), productId );
        return reply.GetIResult();
    }
    static async Task<IResult> Update( CartItemDto item, HttpContext http, ICartRepository repository )
    {
        var reply = await repository.Update( http.UserId(), item.ProductId, item.Quantity );
        return reply.GetIResult();
    }
    static async Task<IResult> Delete( Guid productId, HttpContext http, ICartRepository repository )
    {
        var reply = await repository.Delete( http.UserId(), productId );
        return reply.GetIResult();
    }
    static async Task<IResult> Clear( HttpContext http, ICartRepository repository )
    {
        var reply = await repository.Clear( http.UserId() );
        return reply.GetIResult();
    }
}