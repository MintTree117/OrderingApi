namespace OrderingApplication.Features.User.Login.Types;

internal readonly record struct LoginResponse(
    string? AccessToken,
    bool IsPending2Fa )
{
    internal static LoginResponse LoggedIn( string token ) => 
        new( token, false );
    internal static LoginResponse Pending2Fa() => 
        new( null, true );
}