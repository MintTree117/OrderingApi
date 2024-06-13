namespace OrderingApplication.Features.Users.Security.Types;

internal readonly record struct ViewSecurityResponse(
    bool TwoFactorEnabled,
    string? TwoFactorEmail );