using Microsoft.AspNetCore.Identity;

namespace OrderingDomain.Account;

// This is just so we can con figure IdentityDbContext; to change id type if we want to
public sealed class UserRole : IdentityRole<string> { }