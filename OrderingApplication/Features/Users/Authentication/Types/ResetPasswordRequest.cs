namespace OrderingApplication.Features.Users.Authentication.Types;

internal readonly record struct ResetPasswordRequest(
    string Email,
    string Code,
    string NewPassword,
    string ConfirmPassword );