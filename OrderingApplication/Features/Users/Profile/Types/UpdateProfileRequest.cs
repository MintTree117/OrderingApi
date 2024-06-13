namespace OrderingApplication.Features.Users.Profile.Types;

internal readonly record struct UpdateProfileRequest(
    string Username,
    string Email,
    string? Phone );