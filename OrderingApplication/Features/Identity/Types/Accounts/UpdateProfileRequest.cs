namespace OrderingApplication.Features.Identity.Types.Accounts;

internal readonly record struct UpdateProfileRequest(
    string Username,
    string Email,
    string? Phone );