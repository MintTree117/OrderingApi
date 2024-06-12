namespace OrderingApplication.Features.Account.Login.Types;

internal readonly record struct LoginRequest(
    string EmailOrUsername,
    string Password );