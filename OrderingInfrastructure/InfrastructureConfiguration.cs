using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderingInfrastructure.Email;
using OrderingInfrastructure.Http;
using OrderingInfrastructure.Features.Billing;
using OrderingInfrastructure.Features.Cart;
using OrderingInfrastructure.Features.Users;
using OrderingInfrastructure.Features.Ordering;
using OrderingInfrastructure.Features.Ordering.Repositories;
using OrderingInfrastructure.Features.Users.Repositories;

namespace OrderingInfrastructure;

public static class InfrastructureConfiguration
{
    public static void ConfigureInfrastructure( this WebApplicationBuilder builder )
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSingleton<IEmailSender, EmailSender>();
        builder.Services.AddSingleton<IHttpService, HttpService>();

        builder.Services.AddDbContext<UserDbContext>( GetUserDatabaseOptions );
        builder.Services.AddScoped<IAddressRepository, AddressRepository>();
        builder.Services.AddScoped<ISessionRepository, SessionRepository>();

        builder.Services.AddDbContext<OrderingDbContext>( GetOrderingDatabaseOptions );
        builder.Services.AddScoped<IOrderingRepository, OrderingRepository>();
        builder.Services.AddScoped<IOrderingUtilityRepository, OrderingUtilityRepository>();

        builder.Services.AddSingleton<CartDbContext>();
        builder.Services.AddSingleton<ICartRepository, CartRepository>();

        builder.Services.AddDbContext<BillingDbContext>( GetBillingDatabaseOptions );
        builder.Services.AddScoped<IBillingRepository, BillingRepository>();
    }

    static void GetUserDatabaseOptions( DbContextOptionsBuilder options )
    {
        options.UseInMemoryDatabase( "UserDb" );
    }
    static void GetOrderingDatabaseOptions( DbContextOptionsBuilder options )
    {
        options.UseInMemoryDatabase( "OrderingDb" );
    }
    static void GetBillingDatabaseOptions( DbContextOptionsBuilder options )
    {
        options.UseInMemoryDatabase( "BillingDb" );
    }
}