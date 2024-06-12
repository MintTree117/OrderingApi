using Microsoft.Extensions.Logging;
using OrderingDomain.Billing;
using OrderingDomain.Optionals;

namespace OrderingInfrastructure.Features.Billing;

internal sealed class BillingRepository( BillingDbContext database, ILogger<BillingRepository> logger ) 
    : DatabaseService<BillingRepository>( database,logger ), IBillingRepository
{
    readonly BillingDbContext _database = database;
    
    public Task<Reply<Invoice>> GetInvoice( Guid orderId ) => throw new NotImplementedException();
    public Task<Reply<Bill>> GetBill( Guid orderId ) => throw new NotImplementedException();
    public Task<Reply<bool>> AddInvoice( Invoice invoice ) => throw new NotImplementedException();
    public Task<Reply<bool>> AddBill( Bill bill ) => throw new NotImplementedException();
}