using OrderingApplication.Features.Identity.AccountAddresses;
using OrderingApplication.Features.Identity.AccountDetails;
using OrderingApplication.Features.Identity.AccountSecurity;
using OrderingApplication.Features.Identity.Login;
using OrderingApplication.Features.Identity.Registration;

namespace OrderingApplication.Features.Identity;

internal static class IdentityEndpoints
{
    internal static void MapIdentityEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapLoginEndpoints();
        app.MapRegistrationEndpoints();
        app.MapAccountAddressEndpoints();
        app.MapAccountDetailsEndpoints();;
        app.MapAccountSecurityEndpoints();
    }
}