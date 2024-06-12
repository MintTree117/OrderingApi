using Microsoft.AspNetCore.Identity;

namespace OrderingDomain.Account;

public sealed class UserAccount : IdentityUser<string>
{
    public UserAccount() : base() { }
    public UserAccount( string email, string username ) : base()
    {
        Email = email;
        UserName = username;
    }

    public string? TwoFactorEmail { get; set; }
}