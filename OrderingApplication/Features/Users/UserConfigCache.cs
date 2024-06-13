using System.Text;
using Microsoft.IdentityModel.Tokens;
using OrderingApplication.Extentions;
using OrderingApplication.Features.Users.Utilities;

namespace OrderingApplication.Features.Users;

internal sealed class UserConfigCache( IConfiguration configuration )
{
    const string PagesBase = "User:Pages:";
    internal readonly string ConfirmEmailPage = configuration.GetOrThrow( string.Join( PagesBase, nameof( ConfirmEmailPage ) ) );
    internal readonly string ResetPasswordPage = configuration.GetOrThrow( string.Join( PagesBase, nameof( ResetPasswordPage ) ) );
    
    internal readonly JwtConfig JwtConfigRules = GetJwtRules( configuration );
    const string JwtBase = "User:Jwt:";
    static JwtConfig GetJwtRules( IConfiguration config ) => new() {
        Key = new SymmetricSecurityKey( Encoding.UTF8.GetBytes( config.GetOrThrow( string.Join( JwtBase, nameof( JwtConfig.Key ) ) ) ) ),
        Audience = config.GetOrThrow( string.Join( JwtBase, nameof( JwtConfig.Audience ) ) ),
        Issuer = config.GetOrThrow( string.Join( JwtBase, nameof( JwtConfig.Issuer ) ) ),
        ValidateAudience = config.GetSection( string.Join( JwtBase, nameof( JwtConfig.ValidateAudience ) ) ).Get<bool>(),
        ValidateIssuer = config.GetSection( string.Join( JwtBase, nameof( JwtConfig.ValidateIssuer ) ) ).Get<bool>(),
        ValidateIssuerSigningKey = config.GetSection( string.Join( JwtBase, nameof( JwtConfig.ValidateIssuerSigningKey ) ) ).Get<bool>(),
        AccessLifetime = TimeSpan.Parse( config.GetOrThrow( string.Join( JwtBase, nameof( JwtConfig.AccessLifetime ) ) ) ),
        RefreshLifetime = TimeSpan.Parse( config.GetOrThrow( string.Join( JwtBase, nameof( JwtConfig.RefreshLifetime ) ) ) )
    };
}