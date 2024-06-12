using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderingInfrastructure.Features.Account.Repositories;

namespace OrderingInfrastructure.Features.Account;

internal static class AccountInfrastructureConfiguration
{
    internal static void ConfigureIdentityInfrastructure( this WebApplicationBuilder builder )
    {
        builder.Services.AddDbContext<AccountDbContext>( GetDatabaseOptions );
        builder.Services.AddScoped<IAddressRepository, AddressRepository>();
    }

    static void GetDatabaseOptions( DbContextOptionsBuilder options )
    {
        options.UseInMemoryDatabase( "IdentityDb" );
    }
}