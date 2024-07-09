using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrderingDomain.Users;

namespace OrderingInfrastructure.Features.Users;

public sealed class UserDbContext( DbContextOptions<UserDbContext> options ) 
    : IdentityDbContext<UserAccount>( options )
{
    protected override void OnModelCreating( ModelBuilder modelBuilder )
    {
        base.OnModelCreating( modelBuilder );
        modelBuilder.Entity<UserAccount>().Property( static u => u.Id ).ValueGeneratedOnAdd(); // Configure the table and primary key
        modelBuilder.Entity<UserSession>( static entity => {
            entity.HasKey( static us => us.Id );
            entity.Property( static us => us.UserId )
                .IsRequired();
            entity.Property( static us => us.DateCreated )
                .IsRequired();
            entity.Property( static us => us.LastActive )
                .IsRequired();
        } );

        modelBuilder.Entity<UserAddress>( static entity => {
            entity.HasKey( static ua => ua.Id );
            entity.Property( static ua => ua.UserId )
                .IsRequired();
            entity.Property( static ua => ua.Name )
                .IsRequired();
        } );
    }

    public DbSet<UserAddress> Addresses { get; init; } = default!;
    public DbSet<UserSession> Sessions { get; init; } = default!;
}