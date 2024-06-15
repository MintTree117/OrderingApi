using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OrderingApplication.Features.Billing;
using OrderingApplication.Features.Cart;
using OrderingApplication.Features.Ordering;
using OrderingApplication.Features.Users;
using OrderingApplication.Utilities;
using OrderingDomain.Users;
using OrderingInfrastructure.Features.Users;
using Swashbuckle.AspNetCore.Filters;

namespace OrderingApplication.Extentions;

internal static class BuilderExtentions
{
    internal static void ConfigureFeatures( this WebApplicationBuilder builder )
    {
        builder.ConfigureUsers();
        builder.ConfigureBilling();
        builder.ConfigureCart();
        builder.ConfigureOrdering();
    }
    internal static void ConfigureLogging( this WebApplicationBuilder builder )
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
        EndpointLogger.Logger = builder.Services
            .BuildServiceProvider()
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger<EndpointLogger>();
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
    internal static void ConfigureAuthentication( this WebApplicationBuilder builder )
    {
        builder.Services
            .AddIdentityCore<UserAccount>( options => GetUserOptions( options, builder.Configuration ) )
            .AddEntityFrameworkStores<UserDbContext>()
            .AddDefaultTokenProviders();
        builder.Services
            .AddAuthentication( GetAuthenticationOptions )
            .AddCookie( GetCookieOptions )
            .AddJwtBearer( options => GetJwtOptions( options, builder ) );
        builder.Services
            .AddAuthorization( GetAuthorizationOptions );
    }

    static void GetAuthenticationOptions( AuthenticationOptions options )
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.RequireAuthenticatedSignIn = false;
    }
    static void GetUserOptions( IdentityOptions options, IConfiguration configuration )
    {
        const string Base = "Users:Account:";
        
        options.Stores.ProtectPersonalData = configuration.GetSection( Base + nameof( options.Stores.ProtectPersonalData ) ).Get<bool>();
        
        options.User.RequireUniqueEmail = configuration.GetSection( Base + nameof( options.User.RequireUniqueEmail ) ).Get<bool>();
        options.SignIn.RequireConfirmedEmail = configuration.GetSection( Base + nameof( options.SignIn.RequireConfirmedEmail ) ).Get<bool>();
        options.SignIn.RequireConfirmedAccount = configuration.GetSection( Base + nameof( options.SignIn.RequireConfirmedAccount ) ).Get<bool>();
        options.SignIn.RequireConfirmedPhoneNumber = configuration.GetSection( Base + nameof( options.SignIn.RequireConfirmedPhoneNumber ) ).Get<bool>();
        
        options.Lockout.DefaultLockoutTimeSpan = configuration.GetSection( Base + nameof( options.Lockout.DefaultLockoutTimeSpan ) ).Get<TimeSpan>();
        options.Lockout.MaxFailedAccessAttempts = configuration.GetSection( Base + nameof( options.Lockout.MaxFailedAccessAttempts ) ).Get<int>();
        options.Lockout.AllowedForNewUsers = configuration.GetSection( Base + nameof( options.Lockout.AllowedForNewUsers ) ).Get<bool>();

        const string PasswordBase = "Users:Password:";
        options.Password.RequiredLength = configuration.GetSection( PasswordBase + nameof( options.Password.RequiredLength ) ).Get<int>();
        options.Password.RequireLowercase = configuration.GetSection( PasswordBase + nameof( options.Password.RequireLowercase ) ).Get<bool>();
        options.Password.RequireUppercase = configuration.GetSection( PasswordBase + nameof( options.Password.RequireUppercase ) ).Get<bool>();
        options.Password.RequireDigit = configuration.GetSection( PasswordBase + nameof( options.Password.RequireDigit ) ).Get<bool>();
        options.Password.RequireNonAlphanumeric = configuration.GetSection( PasswordBase + nameof( options.Password.RequireNonAlphanumeric ) ).Get<bool>();
    }
    static void GetAuthorizationOptions( AuthorizationOptions options )
    {
        options.AddPolicy( Consts.DefaultPolicy, static policy => {
            policy.AddAuthenticationSchemes( CookieAuthenticationDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme );
            policy.RequireAssertion( static _ => true ); 
        } );
        
        options.AddPolicy( Consts.CookiePolicy, static policy => {
            policy.AddAuthenticationSchemes( CookieAuthenticationDefaults.AuthenticationScheme );
            policy.RequireAssertion( static _ => true ); 
        } );
        
        options.AddPolicy( Consts.JwtPolicy, static policy => {
            policy.AddAuthenticationSchemes( JwtBearerDefaults.AuthenticationScheme );
            policy.RequireAssertion( static _ => true ); 
            //policy.RequireAuthenticatedUser();
        } );
    }
    static void GetCookieOptions( CookieAuthenticationOptions options )
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes( 30 ); // Expiry time
        // TODO: SETUP SLIDING EXPIRATION EVENTS TO HANDLE MAX CUTOFF DATE
    }
    static void GetJwtOptions( JwtBearerOptions options, WebApplicationBuilder builder )
    {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Users:Jwt:Issuer"],
            ValidAudience = builder.Configuration["Users:Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey( Encoding.UTF8.GetBytes( builder.Configuration["Users:Jwt:Key"] ?? throw new Exception( "Fatal: Failed to get Jwt key from config during startup." ) ) )
        };

        options.SaveToken = true;
    }
}