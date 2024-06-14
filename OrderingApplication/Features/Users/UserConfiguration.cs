using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OrderingApplication.Features.Users.Addresses;
using OrderingApplication.Features.Users.Authentication.Services;
using OrderingApplication.Features.Users.Delete;
using OrderingApplication.Features.Users.Profile;
using OrderingApplication.Features.Users.Registration.Systems;
using OrderingApplication.Features.Users.Security;
using OrderingApplication.Utilities;
using OrderingDomain.Users;
using OrderingInfrastructure.Features.Users;

namespace OrderingApplication.Features.Users;

internal static class UserConfiguration
{
    internal static void ConfigureUsers( this WebApplicationBuilder builder )
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

        builder.Services.AddSingleton<UserConfigCache>();
        builder.Services.AddScoped<UserAddressManager>();
        builder.Services.AddScoped<LoginManager>();
        builder.Services.AddScoped<PasswordResetter>();
        builder.Services.AddScoped<SessionManager>();
        builder.Services.AddScoped<DeleteAccountSystem>();
        builder.Services.AddScoped<AccountProfileManager>();
        builder.Services.AddScoped<AccountConfirmationSystem>();
        builder.Services.AddScoped<AccountRegistrationSystem>();
        builder.Services.AddScoped<AccountSecurityManager>();
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
    static void GetAuthenticationOptions( AuthenticationOptions options )
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.RequireAuthenticatedSignIn = false;
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
        options.ExpireTimeSpan = TimeSpan.FromDays( 1 ); 
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
        
        options.Events = new CookieAuthenticationEvents {
            OnSigningIn = static async context => {
                var principal = context.Principal;
                // Add additional claims if necessary
                await Task.CompletedTask;
            },
            OnSignedIn = static async context => {
                // Optional: Handle post-sign-in actions
                await Task.CompletedTask;
            },
            OnValidatePrincipal = static async context => {
                // Optional: Validate principal periodically
                await Task.CompletedTask;
            }
        };
    }
    static void GetJwtOptions( JwtBearerOptions options, WebApplicationBuilder builder )
    {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Identity:Jwt:Issuer"],
            ValidAudience = builder.Configuration["Identity:Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey( Encoding.UTF8.GetBytes( builder.Configuration["Identity:Jwt:Key"] ?? throw new Exception( "Fatal: Failed to get Jwt key from config during startup." ) ) )
        };

        options.SaveToken = true;
    }
}