namespace OrderingApplication.Features.Account.Registration.Types;

internal readonly record struct ConfirmAccountEmailRequest(
    string Email,
    string Code );