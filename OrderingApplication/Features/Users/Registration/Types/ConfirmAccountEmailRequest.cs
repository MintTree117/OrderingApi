namespace OrderingApplication.Features.Users.Registration.Types;

internal readonly record struct ConfirmAccountEmailRequest(
    string Email,
    string Code );