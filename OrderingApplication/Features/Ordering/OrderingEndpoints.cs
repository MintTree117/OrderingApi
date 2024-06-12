using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Features.Ordering.Dtos;
using OrderingApplication.Features.Ordering.Systems;
using OrderingApplication.Extentions;

namespace OrderingApplication.Features.Ordering;

internal static class OrderingEndpoints
{
    internal static void MapOrderingEndpoints( this IEndpointRouteBuilder app )
    {
        const string baseRoute = "api/post/orders";

        app.MapPost( $"{baseRoute}/place", static async ( [FromBody] OrderPlaceRequest order, OrderPlacingSystem service ) =>
               (await service.PlaceOrder( order )).GetIResult() )
           .RequireAuthorization();

        app.MapPost( $"{baseRoute}/update", static async ( [FromBody] OrderUpdateRequest update, OrderUpdatingSystem service ) =>
               (await service.UpdateOrder( update )).GetIResult() )
           .RequireAuthorization();

        app.MapPost( $"{baseRoute}/cancel", static async ( [FromBody] OrderCancelRequest cancel, OrderCancellingSystem service ) =>
               (await service.CancelOrder( cancel )).GetIResult() )
           .RequireAuthorization();
    }
}