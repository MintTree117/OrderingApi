namespace OrderingApplication.Features.Identity.AccountSecurity.Types;

internal readonly record struct UpdatePasswordRequest(
    string OldPassword,
    string NewPassword );