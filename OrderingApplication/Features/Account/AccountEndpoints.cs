using OrderingApplication.Features.Account.Addresses;
using OrderingApplication.Features.Account.Profile;
using OrderingApplication.Features.Account.Login;
using OrderingApplication.Features.Account.Registration;
using OrderingApplication.Features.Account.Security;

namespace OrderingApplication.Features.Account;

internal static class AccountEndpoints
{
    internal static void MapIdentityEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapLoginEndpoints();
        app.MapRegistrationEndpoints();
        app.MapAccountAddressEndpoints();
        app.MapAccountDetailsEndpoints();
        app.MapAccountSecurityEndpoints();
    }
}