using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrderingDomain.Users;

namespace OrderingInfrastructure.Features.Account;

public sealed class AccountDbContext( DbContextOptions<AccountDbContext> options ) 
    : IdentityDbContext<UserAccount>( options )
{
    protected override void OnModelCreating( ModelBuilder builder )
    {
        base.OnModelCreating( builder );
        builder.Entity<UserAccount>().Property( static u => u.Id ).ValueGeneratedOnAdd(); // Configure the table and primary key
    }

    public DbSet<UserAddress> Addresses { get; init; } = default!;
    public DbSet<UserSession> Sessions { get; init; } = default!;
}