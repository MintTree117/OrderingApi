namespace OrderingApplication.Features.User.Security.Types;

internal readonly record struct ViewSecurityResponse(
    bool TwoFactorEnabled,
    string? TwoFactorEmail );