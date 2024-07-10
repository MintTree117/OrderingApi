using OrderingDomain.Users;

namespace OrderingApplication.Features.Users.Authentication.Types;

public readonly record struct ViewUserSessionsDto(
    int TotalCount,
    List<UserSessionDto> Sessions )
{
    public static ViewUserSessionsDto FromModels( int totalCount, List<UserSession> models )
    {
        List<UserSessionDto> sessions = [];
        sessions.AddRange( from m in models select UserSessionDto.FromModel( m ) );
        return new ViewUserSessionsDto( totalCount, sessions );
    }
}