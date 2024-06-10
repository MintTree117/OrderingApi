using OrderingDomain.Identity;

namespace OrderingApplication.Features.Identity.Types.Accounts;

internal readonly record struct ViewProfileReply(
    string? Username,
    string? Email,
    string? Phone )
{
    internal static ViewProfileReply With( UserAccount user ) => new(
        user.UserName,
        user.Email,
        user.PhoneNumber );
}