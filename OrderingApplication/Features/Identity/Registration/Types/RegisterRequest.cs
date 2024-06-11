namespace OrderingApplication.Features.Identity.Registration.Types;

internal readonly record struct RegisterRequest(
    string Email,
    string Username,
    string? Phone,
    string Password,
    string PasswordConfirm );