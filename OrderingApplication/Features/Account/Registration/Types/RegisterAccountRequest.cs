namespace OrderingApplication.Features.Account.Registration.Types;

internal readonly record struct RegisterAccountRequest(
    string Email,
    string Username,
    string? Phone,
    string Password,
    string PasswordConfirm );