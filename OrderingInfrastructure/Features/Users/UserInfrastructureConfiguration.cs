using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderingInfrastructure.Features.Users.Repositories;

namespace OrderingInfrastructure.Features.Users;

internal static class UserInfrastructureConfiguration
{
    internal static void ConfigureUserInfrastructure( this WebApplicationBuilder builder )
    {
        builder.Services.AddDbContext<UserDbContext>( GetDatabaseOptions );
        builder.Services.AddScoped<IAddressRepository, AddressRepository>();
        builder.Services.AddScoped<ISessionRepository, SessionRepository>();
    }

    static void GetDatabaseOptions( DbContextOptionsBuilder options )
    {
        options.UseInMemoryDatabase( "UserDb" );
    }
}