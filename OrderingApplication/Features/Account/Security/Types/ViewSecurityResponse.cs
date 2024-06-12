namespace OrderingApplication.Features.Account.Security.Types;

internal readonly record struct ViewSecurityResponse(
    bool TwoFactorEnabled );