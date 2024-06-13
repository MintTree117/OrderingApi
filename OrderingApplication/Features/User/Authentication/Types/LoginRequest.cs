namespace OrderingApplication.Features.User.Authentication.Types;

internal readonly record struct LoginRequest(
    string EmailOrUsername,
    string Password );