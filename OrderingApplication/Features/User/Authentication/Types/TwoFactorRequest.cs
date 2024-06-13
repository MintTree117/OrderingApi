namespace OrderingApplication.Features.User.Authentication.Types;

internal readonly record struct TwoFactorRequest(
    string EmailOrUsername,
    string Code );