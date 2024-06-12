namespace OrderingApplication.Features.User.Login.Types;

internal readonly record struct TwoFactorRequest(
    string EmailOrUsername,
    string Code );