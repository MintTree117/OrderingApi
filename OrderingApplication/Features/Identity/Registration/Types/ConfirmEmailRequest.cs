namespace OrderingApplication.Features.Identity.Registration.Types;

internal readonly record struct ConfirmEmailRequest(
    string Email,
    string Code );