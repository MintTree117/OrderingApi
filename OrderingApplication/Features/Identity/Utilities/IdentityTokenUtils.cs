using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using OrderingDomain.Optionals;

namespace OrderingApplication.Features.Identity.Utilities;

internal static class IdentityTokenUtils
{
    internal static string GenerateAccessToken( string userId, JwtConfig jwtConfig )
    {
        DateTime expiration = DateTime.UtcNow + jwtConfig.AccessLifetime;
        SigningCredentials credentials = new( jwtConfig.Key, SecurityAlgorithms.HmacSha256 );
        Claim[] claims = [new Claim( ClaimTypes.NameIdentifier, userId)];
        JwtSecurityToken token = new(
            null, // single issuer, no need
            jwtConfig.Audience,
            claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken( token );
    }
    internal static Reply<bool> ParseToken( string? token, JwtConfig config, out ClaimsPrincipal? claimsPrincipal, out JwtSecurityToken? jwt )
    {
        claimsPrincipal = null;
        jwt = null;

        try
        {
            if (string.IsNullOrWhiteSpace( token ))
                return IReply.None( "Tried to parse a null token." );
            claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken( token, new TokenValidationParameters {
                ValidateIssuerSigningKey = config.ValidateIssuerSigningKey,
                IssuerSigningKey = config.Key,
                ValidateIssuer = config.ValidateIssuer,
                ValidateAudience = config.ValidateAudience,
                ValidIssuer = config.Issuer,
                ValidAudience = config.Audience,
                ClockSkew = TimeSpan.FromMinutes( 5 )
            }, out SecurityToken validatedToken );

            jwt = (JwtSecurityToken) validatedToken;
            return IReply.Okay();
        }
        catch ( Exception e )
        {
            return IReply.None( $"{e} An exception occurred while validating a json web token." );
        }
    }
}