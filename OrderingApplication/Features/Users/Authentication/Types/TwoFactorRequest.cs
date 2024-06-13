namespace OrderingApplication.Features.Users.Authentication.Types;

internal readonly record struct TwoFactorRequest(
    string EmailOrUsername,
    string Code );