using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Identity.AccountAddresses.Types;

namespace OrderingApplication.Features.Identity.AccountAddresses;

internal static class UserAddressEndpoints
{
    const string CookiesOrJwt = "CookiesOrJwt";
    
    internal static void MapAccountAddressEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapGet( "api/account/address/view", static async ( [FromQuery] int page, [FromQuery] int pageSize, HttpContext http, UserAddressManager system ) =>
            (await system.ViewAddresses( http.UserId(), page, pageSize ))
            .GetIResult() ).RequireAuthorization( CookiesOrJwt );
        app.MapPut( "api/account/address/add", static async ( [FromBody] AddressDto request, HttpContext http, UserAddressManager system ) =>
            (await system.AddAddress( http.UserId(), request ))
            .GetIResult() ).RequireAuthorization( CookiesOrJwt );
        app.MapPost( "api/account/address/update", static async ( [FromBody] AddressDto request, HttpContext http, UserAddressManager system ) =>
            (await system.UpdateAddress( http.UserId(), request ))
            .GetIResult() ).RequireAuthorization( CookiesOrJwt );
        app.MapDelete( "api/account/address/delete", static async ( [FromQuery] Guid addressId, HttpContext http, UserAddressManager system ) =>
            (await system.DeleteAddress( http.UserId(), addressId ))
            .GetIResult() ).RequireAuthorization( CookiesOrJwt );
    }
}