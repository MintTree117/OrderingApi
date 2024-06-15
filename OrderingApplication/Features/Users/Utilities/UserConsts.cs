using System.Text;
using Microsoft.IdentityModel.Tokens;
using OrderingApplication.Extentions;

namespace OrderingApplication.Features.Users.Utilities;

internal sealed class UserConsts( IConfiguration configuration )
{
    internal static UserConsts Instance { get; set; } = default!;
    
    const string PagesBase = "Users:Pages:";
    internal readonly string ConfirmEmailPage = configuration.GetOrThrow( PagesBase + nameof( ConfirmEmailPage ) );
    internal readonly string ResetPasswordPage = configuration.GetOrThrow( PagesBase + nameof( ResetPasswordPage ) );
    
    internal readonly JwtConfig JwtConfigRules = GetJwtRules( configuration );
    const string JwtBase = "Users:Jwt:";
    static JwtConfig GetJwtRules( IConfiguration config ) => new() {
        Key = GetJwtKey( config ),
        Audience = config.GetOrThrow( JwtBase + nameof( JwtConfig.Audience ) ),
        Issuer = config.GetOrThrow( JwtBase + nameof( JwtConfig.Issuer ) ),
        ValidateAudience = config.GetSection( JwtBase + nameof( JwtConfig.ValidateAudience ) ).Get<bool>(),
        ValidateIssuer = config.GetSection( JwtBase + nameof( JwtConfig.ValidateIssuer ) ).Get<bool>(),
        ValidateIssuerSigningKey = config.GetSection( JwtBase + nameof( JwtConfig.ValidateIssuerSigningKey ) ).Get<bool>(),
        AccessLifetime = TimeSpan.Parse( config.GetOrThrow( JwtBase + nameof( JwtConfig.AccessLifetime ) ) )
    };
    static SymmetricSecurityKey GetJwtKey( IConfiguration config )
    {
        string keyString = config.GetOrThrow( JwtBase + nameof( JwtConfig.Key ) );
        byte[] keyBytes = Encoding.UTF8.GetBytes( keyString );
        return new SymmetricSecurityKey( keyBytes );
    }
}