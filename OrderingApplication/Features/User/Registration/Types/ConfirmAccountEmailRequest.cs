namespace OrderingApplication.Features.User.Registration.Types;

internal readonly record struct ConfirmAccountEmailRequest(
    string Email,
    string Code );