namespace OrderingApplication.Features.User.Authentication.Types;

internal readonly record struct TwoFactorResponse(
    string AccessToken )
{
    internal static TwoFactorResponse Authenticated( string token ) =>
        new( token );
}