namespace OrderingApplication.Features.Users.Security.Types;

internal readonly record struct UpdatePasswordRequest(
    string OldPassword,
    string NewPassword );