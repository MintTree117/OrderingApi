using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrderingDomain.Account;

namespace OrderingInfrastructure.Features.Account;

public sealed class AccountDbContext : IdentityDbContext<UserAccount, UserRole, string>
{
    public AccountDbContext( DbContextOptions<AccountDbContext> options ) : base( options ) { }
    protected override void OnModelCreating( ModelBuilder builder )
    {
        base.OnModelCreating( builder );
        builder.Entity<UserAccount>().Property( u => u.Id ).ValueGeneratedOnAdd(); // Configure the table and primary key
    }
    public DbSet<UserAddress> Addresses { get; set; }
    public DbSet<UserSession> Sessions { get; set; }
}