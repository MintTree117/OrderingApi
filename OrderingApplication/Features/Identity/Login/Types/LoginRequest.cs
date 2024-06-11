namespace OrderingApplication.Features.Identity.Login.Types;

internal readonly record struct LoginRequest(
    string EmailOrUsername,
    string Password );