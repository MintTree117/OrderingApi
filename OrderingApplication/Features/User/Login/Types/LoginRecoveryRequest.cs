namespace OrderingApplication.Features.User.Login.Types;

internal readonly record struct LoginRecoveryRequest(
    string EmailOrUsername,
    string RecoveryCode );