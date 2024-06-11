namespace OrderingApplication.Features.Identity.Login.Types;

internal readonly record struct TwoFactorResponse(
    string AccessToken )
{
    internal static TwoFactorResponse Authenticated( string token ) =>
        new( token );
}