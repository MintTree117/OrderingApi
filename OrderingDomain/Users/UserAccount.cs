using Microsoft.AspNetCore.Identity;

namespace OrderingDomain.Users;

public sealed class UserAccount : IdentityUser
{
    public UserAccount() : base() { }
    public UserAccount( string email, string username, string? phone ) : base()
    {
        Email = email;
        UserName = username;
        PhoneNumber = phone;
    }

    public string? TwoFactorEmail { get; set; }
}