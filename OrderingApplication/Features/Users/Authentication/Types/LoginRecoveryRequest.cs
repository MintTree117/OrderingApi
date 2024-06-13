namespace OrderingApplication.Features.Users.Authentication.Types;

internal readonly record struct LoginRecoveryRequest(
    string EmailOrUsername,
    string RecoveryCode );