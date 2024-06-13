using Microsoft.OpenApi.Models;
using OrderingApplication.Features.Billing;
using OrderingApplication.Features.Cart;
using OrderingApplication.Features.Users;
using OrderingApplication.Features.Ordering;
using Swashbuckle.AspNetCore.Filters;

namespace OrderingApplication.Extentions;

internal static class BuilderExtentions
{
    internal static void ConfigureLogging( this WebApplicationBuilder builder )
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
    }
    internal static void ConfigureFeatures( this WebApplicationBuilder builder )
    {
        builder.ConfigureUsers();
        builder.ConfigureBilling();
        builder.ConfigureCart();
        builder.ConfigureOrdering();
    }
    internal static void ConfigureSwagger( this WebApplicationBuilder builder )
    {
        builder.Services.AddSwaggerGen( static options => {
            options.AddSecurityDefinition( "oauth2", new OpenApiSecurityScheme() {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
                Type = SecuritySchemeType.ApiKey
            } );
            options.OperationFilter<SecurityRequirementsOperationFilter>();
        } );
    }
    internal static void ConfigureCors( this WebApplicationBuilder builder )
    {
        builder.Services.AddCors( static options => {
            options.AddDefaultPolicy( static cors => cors
                        .WithOrigins( "https://localhost:7221", "https://localhost:7212" )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials() );
        } );
    }
}