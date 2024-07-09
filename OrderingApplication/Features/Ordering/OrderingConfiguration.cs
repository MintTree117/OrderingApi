using OrderingApplication.Features.Ordering.Services;

namespace OrderingApplication.Features.Ordering;

internal static class OrderingConfiguration
{
    internal static void ConfigureOrdering( this WebApplicationBuilder builder )
    {
        builder.Services.AddScoped<WarehouseOrderingSystem>();
        builder.Services.AddScoped<CustomerOrderingSystem>();
    }
}