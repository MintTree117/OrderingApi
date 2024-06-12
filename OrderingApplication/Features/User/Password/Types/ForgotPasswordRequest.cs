namespace OrderingApplication.Features.User.Password.Types;

internal readonly record struct ForgotPasswordRequest(
    string Email );