using OrderingApplication.Features.Billing;
using OrderingApplication.Features.Cart;
using OrderingApplication.Features.Users;
using OrderingApplication.Features.Ordering;

namespace OrderingApplication.Extentions;

internal static class AppExtentions
{
    internal static void UseSwagger( this WebApplication app )
    {
        if (!app.Environment.IsDevelopment()) 
            return;
        
        SwaggerBuilderExtensions.UseSwagger( app );
        app.UseSwaggerUI();
    }
    internal static void UseEndpoints( this WebApplication app )
    {
        app.MapUserEndpoints();
        app.MapBillingEndpoints();
        app.MapCartEndpoints();
        app.MapOrderingEndpoints();
    }
}