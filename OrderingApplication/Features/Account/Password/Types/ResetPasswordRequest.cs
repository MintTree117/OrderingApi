namespace OrderingApplication.Features.Account.Password.Types;

internal readonly record struct ResetPasswordRequest(
    string Email,
    string Code,
    string NewPassword,
    string ConfirmPassword );