namespace OrderingApplication.Features.User.Authentication.Types;

internal readonly record struct LoginRecoveryRequest(
    string EmailOrUsername,
    string RecoveryCode );