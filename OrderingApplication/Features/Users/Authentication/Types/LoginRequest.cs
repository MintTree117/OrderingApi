namespace OrderingApplication.Features.Users.Authentication.Types;

internal readonly record struct LoginRequest(
    string EmailOrUsername,
    string Password );