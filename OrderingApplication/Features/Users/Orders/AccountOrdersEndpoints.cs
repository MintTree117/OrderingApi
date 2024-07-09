using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Utilities;

namespace OrderingApplication.Features.Users.Orders;

internal static class AccountOrdersEndpoints
{
    const int PageSize = 10;
    
    internal static void MapAccountOrdersEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapGet( "api/account/orders/view",
            static async ( [FromQuery] int page, HttpContext http, AccountOrdersService service ) =>
                await ViewPaginatedOrders( page, http, service ) ).RequireAuthorization( Consts.DefaultPolicy );

        app.MapPut( "api/account/orders/details",
            static async ( [FromBody] Guid orderId, HttpContext http, AccountOrdersService service ) =>
            await ViewOrderDetails( orderId, http, service ) ).RequireAuthorization( Consts.DefaultPolicy );
    }

    static async Task<IResult> ViewPaginatedOrders( int page, HttpContext http, AccountOrdersService service )
    {
        var reply = await service.ViewPaginatedOrders( http.UserId(), page, PageSize );
        return reply
            ? Results.Ok( reply.Data )
            : Results.Problem( reply.GetMessage() );
    }
    static async Task<IResult> ViewOrderDetails( Guid orderId, HttpContext http, AccountOrdersService service )
    {
        var reply = await service.ViewOrderDetails( http.UserId(), orderId );
        return reply
            ? Results.Ok( reply.Data )
            : Results.Problem( reply.GetMessage() );
    }
}