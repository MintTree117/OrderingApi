using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OrderingApplication.Features.Users.Addresses;
using OrderingApplication.Features.Users.Authentication.Services;
using OrderingApplication.Features.Users.Profile;
using OrderingApplication.Features.Users.Registration.Systems;
using OrderingApplication.Features.Users.Security;
using OrderingApplication.Features.Users.Utilities;
using OrderingApplication.Utilities;
using OrderingDomain.Users;
using OrderingInfrastructure.Features.Account;

namespace OrderingApplication.Features.Users;

internal static class UserConfiguration
{
    internal static void ConfigureUsers( this WebApplicationBuilder builder )
    {
        builder.Services
               .AddIdentityCore<UserAccount>( options => GetUserOptions( options, builder.Configuration ) )
               .AddEntityFrameworkStores<AccountDbContext>()
               .AddDefaultTokenProviders();
        builder.Services
               .AddAuthentication( GetAuthenticationOptions )
               .AddCookie( GetCookieOptions )
               .AddJwtBearer( options => GetJwtOptions( options, builder ) );
        builder.Services
               .AddAuthorization( GetAuthorizationOptions );
        builder.Services.AddSingleton<UserConfigCache>();
        builder.Services.AddScoped<LoginManager>();
        builder.Services.AddScoped<AccountConfirmationSystem>();
        builder.Services.AddScoped<AccountRegistrationSystem>();
        builder.Services.AddScoped<AccountProfileManager>();
        builder.Services.AddScoped<AccountSecurityManager>();
        builder.Services.AddScoped<UserAddressManager>();
    }

    static void GetUserOptions( IdentityOptions options, IConfiguration configuration )
    {
        options.Stores.ProtectPersonalData = configuration.GetSection( "Identity:User:ProtectPersonalData" ).Get<bool>();
        
        options.User.RequireUniqueEmail = configuration.GetSection( "Identity:User:RequireConfirmedEmail" ).Get<bool>();
        options.User.AllowedUserNameCharacters = configuration.GetSection( "Identity:User:AllowedUserNameCharacters" ).Get<string>() ?? "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@";
        
        options.SignIn.RequireConfirmedEmail = configuration.GetSection( "Identity:SignIn:RequireConfirmedEmail" ).Get<bool>();
        options.SignIn.RequireConfirmedAccount = configuration.GetSection( "Identity:SignIn:RequireConfirmedEmail" ).Get<bool>();
        options.SignIn.RequireConfirmedPhoneNumber = configuration.GetSection( "Identity:SignIn:RequireConfirmedEmail" ).Get<bool>();

        IConfigurationSection passwordSection = configuration.GetSection( "Identity:Validation:PasswordCriteria:" );
        options.Password.RequiredLength = passwordSection.GetSection( "MinLength" ).Get<int>();
        options.Password.RequireLowercase = passwordSection.GetSection( "RequireLowercase" ).Get<bool>();
        options.Password.RequireUppercase = passwordSection.GetSection( "RequireUppercase" ).Get<bool>();
        options.Password.RequireDigit = passwordSection.GetSection( "RequireDigit" ).Get<bool>();
        options.Password.RequireNonAlphanumeric = passwordSection.GetSection( "RequireSpecial" ).Get<bool>();

        IConfigurationSection lockoutSection = configuration.GetSection( "Identity:Lockout" );
        options.Lockout.DefaultLockoutTimeSpan = lockoutSection.GetSection( "DefaultLockoutTimeSpan" ).Get<TimeSpan>();
        options.Lockout.MaxFailedAccessAttempts = lockoutSection.GetSection( "MaxFailedAccessAttempts" ).Get<int>();
        options.Lockout.AllowedForNewUsers = lockoutSection.GetSection( "AllowedForNewUsers" ).Get<bool>();
    }
    static void GetAuthenticationOptions( AuthenticationOptions options )
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    }
    static void GetAuthorizationOptions( AuthorizationOptions options )
    {
        options.AddPolicy( Consts.DefaultPolicy, static policy => {
            policy.AddAuthenticationSchemes( CookieAuthenticationDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme );
            policy.RequireAuthenticatedUser();
        } );
        
        options.AddPolicy( Consts.CookiePolicy, static policy => {
            policy.AddAuthenticationSchemes( CookieAuthenticationDefaults.AuthenticationScheme );
            policy.RequireAuthenticatedUser();
        } );
        
        options.AddPolicy( Consts.JwtPolicy, static policy => {
            policy.AddAuthenticationSchemes( JwtBearerDefaults.AuthenticationScheme );
            policy.RequireAuthenticatedUser();
        } );
    }
    static void GetCookieOptions( CookieAuthenticationOptions options )
    {
        options.ExpireTimeSpan = TimeSpan.FromDays( 1 ); 
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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