using OrderingApplication.Features.Ordering.Services;

namespace OrderingApplication.Features.Ordering;

internal static class OrderingConfiguration
{
    internal static void ConfigureOrdering( this WebApplicationBuilder builder )
    {
        builder.Services.AddScoped<CustomerOrderingSystem>();
        builder.Services.AddScoped<WarehouseOrderingSystem>();
    }
}