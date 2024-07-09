using OrderingApplication.Features.Users.Addresses;
using OrderingApplication.Features.Users.Authentication.Services;
using OrderingApplication.Features.Users.Delete;
using OrderingApplication.Features.Users.Orders;
using OrderingApplication.Features.Users.Profile;
using OrderingApplication.Features.Users.Registration.Systems;
using OrderingApplication.Features.Users.Security;
using OrderingApplication.Features.Users.Utilities;

namespace OrderingApplication.Features.Users;

internal static class UserConfiguration
{
    internal static void ConfigureUsers( this WebApplicationBuilder builder )
    {
        UserConsts.Instance = new UserConsts( builder.Configuration );
        
        builder.Services.AddScoped<UserAddressManager>();
        builder.Services.AddScoped<LoginManager>();
        builder.Services.AddScoped<PasswordResetter>();
        builder.Services.AddScoped<SessionManager>();
        builder.Services.AddScoped<DeleteAccountSystem>();
        builder.Services.AddScoped<AccountProfileManager>();
        builder.Services.AddScoped<AccountConfirmationSystem>();
        builder.Services.AddScoped<AccountRegistrationSystem>();
        builder.Services.AddScoped<AccountSecurityManager>();
        builder.Services.AddScoped<AccountOrdersService>();
    }
}