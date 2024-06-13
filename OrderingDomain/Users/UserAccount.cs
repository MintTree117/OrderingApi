using Microsoft.AspNetCore.Identity;

namespace OrderingDomain.Users;

public sealed class UserAccount : IdentityUser
{
    public UserAccount() : base() { }
    public UserAccount( string email, string username ) : base()
    {
        Email = email;
        UserName = username;
    }

    public string? TwoFactorEmail { get; set; }
}