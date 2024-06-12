namespace OrderingApplication.Features.Account.Security.Types;

internal readonly record struct UpdatePasswordRequest(
    string OldPassword,
    string NewPassword );