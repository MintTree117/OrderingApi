using OrderingDomain.Billing;
using OrderingDomain.Orders.Base;
using OrderingDomain.ReplyTypes;

namespace OrderingApplication.Features.Billing;

internal sealed class BillingService
{
    internal async Task<Invoice> CreateInvoice()
    {
        await Task.Delay( 1000 );
        throw new Exception();
    }
    internal async Task<Bill> CreateBill()
    {
        await Task.Delay( 1000 );
        throw new Exception();
    }
    internal async Task<Reply<bool>> SendInvoice( Order order )
    {
        await Task.Delay( 1000 );
        throw new Exception();
    }
    internal async Task<Reply<bool>> SendBill( Order order )
    {
        await Task.Delay( 1000 );
        throw new Exception();
    }
}