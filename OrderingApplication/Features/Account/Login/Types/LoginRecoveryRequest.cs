namespace OrderingApplication.Features.Account.Login.Types;

internal readonly record struct LoginRecoveryRequest(
    string EmailOrUsername,
    string RecoveryCode );