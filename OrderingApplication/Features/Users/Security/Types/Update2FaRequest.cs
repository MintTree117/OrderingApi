namespace OrderingApplication.Features.Users.Security.Types;

internal readonly record struct Update2FaRequest(
    string? TwoFactorEmail );