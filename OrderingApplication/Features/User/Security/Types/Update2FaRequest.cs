namespace OrderingApplication.Features.User.Security.Types;

internal readonly record struct Update2FaRequest(
    bool IsEnabled,
    string? TwoFactorEmail );