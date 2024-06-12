namespace OrderingApplication.Features.Account.Login.Types;

internal readonly record struct TwoFactorRequest(
    string EmailOrUsername,
    string Code );