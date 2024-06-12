namespace OrderingApplication.Features.Account.Recovery;

internal readonly record struct ForgotPasswordRequest(
    string Email );