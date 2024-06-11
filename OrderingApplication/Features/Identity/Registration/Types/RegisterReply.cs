using OrderingDomain.Identity;

namespace OrderingApplication.Features.Identity.Registration.Types;

internal readonly record struct RegisterReply(
    string Id,
    string Email,
    string Username )
{
    internal static RegisterReply With( UserAccount user ) => new(
        user.Id,
        user.Email ?? string.Empty,
        user.UserName ?? string.Empty );
}