using OrderingDomain.Users;

namespace OrderingApplication.Features.Users.Authentication.Types;

public readonly record struct UserSessionDto(
    string SessionId,
    DateTime LastActivityDate,
    string SessionInformation )
{
    public static UserSessionDto FromModel( UserSession model ) =>
        new( model.Id, model.LastActive, model.IpAddress ?? string.Empty );
}