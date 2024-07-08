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
        
        app.MapPost( "api/orders/place",
                static async ( [FromBody] OrderPlacementRequest place, HttpContext http, OrderingSystem system ) =>
                await PlaceOrder( place, http, system ) )
            .RequireAuthorization( Consts.DefaultPolicy );

        app.MapPost( "api/orders/update",
                static async ( [FromBody] OrderUpdateRequest update, OrderingSystem system ) =>
                await UpdateOrder( update, system ) )
            .RequireAuthorization( Consts.DefaultPolicy );

        app.MapPost( "api/orders/cancel",
                static async ( [FromBody] OrderCancelRequest cancel, OrderingSystem system ) =>
                await CancelOrder( cancel, system ) )
            .RequireAuthorization( Consts.DefaultPolicy );
    }

    static async Task<IResult> GetNewWarehouseOrders( string warehouseToken, HttpContext http, WarehouseOrderingSystem system )
    {
        return Results.Ok();
    }
    static async Task<IResult> PlaceOrder( OrderPlacementRequest order, HttpContext http, OrderingSystem system )
    {
        var reply = await system.PlaceOrder( http.UserId(), order );
        return reply
            ? Results.Ok( true )
            : Results.Problem( reply.GetMessage() );
    }
    static async Task<IResult> UpdateOrder( OrderUpdateRequest update, OrderingSystem system )
    {
        var reply = await system.UpdateOrder( update );
        return reply
            ? Results.Ok( true )
            : Results.Problem( reply.GetMessage() );
    }
    static async Task<IResult> CancelOrder( OrderCancelRequest cancel, OrderingSystem system )
    {
        var reply = await system.CancelOrder( cancel );
        return reply
            ? Results.Ok( true )
            : Results.Problem( reply.GetMessage() );
    }
}