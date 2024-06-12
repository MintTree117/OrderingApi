namespace OrderingApplication.Features.User.Security.Types;

internal readonly record struct UpdatePasswordRequest(
    string OldPassword,
    string NewPassword );