namespace OrderingApplication.Features.Identity.Login.Types;

internal readonly record struct TwoFactorRequest(
    string EmailOrUsername,
    string Code );