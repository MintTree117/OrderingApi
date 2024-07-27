using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Features.Ordering.Dtos;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Ordering.Services;
using OrderingApplication.Utilities;

namespace OrderingApplication.Features.Ordering;

internal static class OrderingEndpoints
{
    internal static void MapOrderingEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapPost( "api/orders/getNew",
                static async ( [FromBody] string warehouseToken, HttpContext http, WarehouseOrderingSystem system ) =>
                await GetNewWarehouseOrders( warehouseToken, http, system ) )
            .RequireAuthorization( Consts.DefaultPolicy );

        app.MapPost( "api/orders/place/guest",
            static async ( [FromBody] OrderPlacementRequest order, CustomerOrderingSystem system ) =>
            await PlaceOrderForGuest( order, system ) );
        
        app.MapPost( "api/orders/place/user",
                static async ( [FromBody] OrderPlacementRequest order, HttpContext http, CustomerOrderingSystem system ) =>
                await PlaceOrderForUser( order, http, system ) )
            .RequireAuthorization( Consts.DefaultPolicy );

        app.MapPost( "api/orders/update",
                static async ( [FromBody] OrderUpdateRequest update, WarehouseOrderingSystem system ) =>
                await UpdateOrder( update, system ) )
            .RequireAuthorization( Consts.DefaultPolicy );

        app.MapPost( "api/orders/cancel",
                static async ( [FromBody] OrderCancelRequest cancel, CustomerOrderingSystem system ) =>
                await CancelOrder( cancel, system ) )
            .RequireAuthorization( Consts.DefaultPolicy );
    }

    static async Task<IResult> GetNewWarehouseOrders( string warehouseToken, HttpContext http, WarehouseOrderingSystem system )
    {
        // TODO: Implement
        await Task.Delay( 1000 );
        return Results.Ok();
    }
    static async Task<IResult> PlaceOrderForGuest( OrderPlacementRequest order, CustomerOrderingSystem system )
    {
        var reply = await system.PlaceOrderForGuest( order );
        return reply
            ? Results.Ok( true )
            : Results.Problem( reply.GetMessage() );
    }
    static async Task<IResult> PlaceOrderForUser( OrderPlacementRequest order, HttpContext http, CustomerOrderingSystem system )
    {
        var reply = await system.PlaceOrderForUser( http.UserId(), order );
        return reply
            ? Results.Ok( true )
            : Results.Problem( reply.GetMessage() );
    }
    static async Task<IResult> UpdateOrder( OrderUpdateRequest update, WarehouseOrderingSystem system )
    {
        var reply = await system.UpdateOrder( update );
        return reply
            ? Results.Ok( true )
            : Results.Problem( reply.GetMessage() );
    }
    static async Task<IResult> CancelOrder( OrderCancelRequest cancel, CustomerOrderingSystem system )
    {
        var reply = await system.CancelOrder( cancel );
        return reply
            ? Results.Ok( true )
            : Results.Problem( reply.GetMessage() );
    }
}