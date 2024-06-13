using OrderingDomain.Users;

namespace OrderingApplication.Features.Users.Registration.Types;

internal readonly record struct RegisterAccountResponse(
    string Id,
    string Email,
    string Username )
{
    internal static RegisterAccountResponse With( UserAccount user ) => new(
        user.Id,
        user.Email ?? string.Empty,
        user.UserName ?? string.Empty );
}