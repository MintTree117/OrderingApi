using OrderingDomain.Identity;

namespace OrderingApplication.Features.Identity.AccountDetails.Types;

internal readonly record struct ViewDetailsResponse(
    string? Username,
    string? Email,
    string? Phone )
{
    internal static ViewDetailsResponse With( UserAccount user ) => new(
        user.UserName,
        user.Email,
        user.PhoneNumber );
}