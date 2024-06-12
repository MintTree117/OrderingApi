namespace OrderingApplication.Features.Account.Recovery.Types;

internal readonly record struct ResetPasswordRequest(
    string Email,
    string Code,
    string NewPassword,
    string ConfirmPassword );