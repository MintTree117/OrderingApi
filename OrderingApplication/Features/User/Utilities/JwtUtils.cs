using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using OrderingDomain.Account;
using OrderingDomain.ReplyTypes;

namespace OrderingApplication.Features.User.Utilities;

internal static class JwtUtils
{
    internal static string GenerateAccessToken( UserAccount user, JwtConfig jwtConfig )
    {
        DateTime expiration = DateTime.UtcNow + jwtConfig.AccessLifetime;
        SigningCredentials credentials = new( jwtConfig.Key, SecurityAlgorithms.HmacSha256 );
        Claim[] claims = [
            new Claim( ClaimTypes.NameIdentifier, user.Id),
            new Claim( ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id )];
        JwtSecurityToken token = new(
            null, // single issuer, no need to validate
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
                return IReply.Fail( "Tried to parse a null token." );
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
            return IReply.Success();
        }
        catch ( Exception e )
        {
            return IReply.Fail( $"{e} An exception occurred while validating a json web token." );
        }
    }
}