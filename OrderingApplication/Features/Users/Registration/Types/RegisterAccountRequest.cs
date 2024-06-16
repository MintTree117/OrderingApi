namespace OrderingApplication.Features.Users.Registration.Types;

internal readonly record struct RegisterAccountRequest(
    string Email,
    string Username,
    string? Phone,
    string Password,
    string PasswordConfirm,
    string? TwoFactorEmail );