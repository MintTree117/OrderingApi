namespace OrderingApplication.Features.Identity.AccountDetails.Types;

internal readonly record struct UpdateDetailsRequest(
    string Username,
    string Email,
    string? Phone );