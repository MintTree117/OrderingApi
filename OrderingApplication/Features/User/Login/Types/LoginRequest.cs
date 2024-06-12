namespace OrderingApplication.Features.User.Login.Types;

internal readonly record struct LoginRequest(
    string EmailOrUsername,
    string Password );