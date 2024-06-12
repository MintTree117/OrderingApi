using OrderingDomain.Account;

namespace OrderingApplication.Features.Account.Profile.Types;

internal readonly record struct ViewProfileResponse(
    string? Username,
    string? Email,
    string? Phone )
{
    internal static ViewProfileResponse With( UserAccount user ) => new(
        user.UserName,
        user.Email,
        user.PhoneNumber );
}