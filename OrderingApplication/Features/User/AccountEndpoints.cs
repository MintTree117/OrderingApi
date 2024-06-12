using OrderingApplication.Features.User.Addresses;
using OrderingApplication.Features.User.Delete;
using OrderingApplication.Features.User.Profile;
using OrderingApplication.Features.User.Login;
using OrderingApplication.Features.User.Registration;
using OrderingApplication.Features.User.Security;

namespace OrderingApplication.Features.User;

internal static class AccountEndpoints
{
    internal static void MapIdentityEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapLoginEndpoints();
        app.MapRegistrationEndpoints();
        app.MapAccountProfileEndpoints();
        app.MapAccountSecurityEndpoints();
        app.MapAccountAddressEndpoints();
        app.MapAccountDeleteEndpoints();
    }
}