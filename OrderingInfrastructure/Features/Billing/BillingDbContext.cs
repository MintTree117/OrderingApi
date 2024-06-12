using Microsoft.EntityFrameworkCore;
using OrderingDomain.Billing;

namespace OrderingInfrastructure.Features.Billing;

public class BillingDbContext : DbContext
{
    public BillingDbContext( DbContextOptions<BillingDbContext> options ) : base( options ) { }

    public DbSet<Invoice> Invoices { get; set; } = default!;
    public DbSet<Bill> Bills { get; set; } = default!;
}