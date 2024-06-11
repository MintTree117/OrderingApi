namespace OrderingApplication.Features.Identity.AccountSecurity.Types;

internal readonly record struct ResetPasswordRequest(
    string Email,
    string Code,
    string NewPassword,
    string ConfirmPassword );