using OrderingDomain.Orders;
using OrderingDomain.ReplyTypes;
using OrderingInfrastructure.Features.Ordering.Repositories;

namespace OrderingApplication.Features.Ordering.Services;

internal sealed class OrderPendingCancelService( IServiceProvider serviceProvider, IConfiguration configuration ) : BackgroundService
{
    readonly IServiceProvider _serviceProvider = serviceProvider;
    readonly TimeSpan _refreshInterval = GetRefreshInterval( configuration );

    protected override async Task ExecuteAsync( CancellationToken stoppingToken )
    {
        using PeriodicTimer timer = new( _refreshInterval );
        try {
            while ( await timer.WaitForNextTickAsync( stoppingToken ) )
                await Handle();
        }
        catch ( OperationCanceledException ) {
            Console.WriteLine( "Timed Hosted Service is stopping." );
        }
    }
    async Task Handle()
    {
        await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetService<IOrderingUtilityRepository>()!;
        var loc = scope.ServiceProvider.GetService<OrderLocationService>()!;

        if ((await repo.GetPendingCancelLines())
            .OutFailure( out Replies<OrderLine> linesOpt ))
            Console.WriteLine( linesOpt.GetMessage() );

        foreach ( OrderLine l in linesOpt.Enumerable )
            if ((await CancelLine( l, loc )).OutSuccess( out Reply<bool> s ))
                await repo.DeletePendingDeleteLine( l );
    }
    async Task<Reply<bool>> CancelLine( OrderLine line, OrderLocationService loc ) =>
        (await loc.ConfirmCancelOrderLine( line ))
        .OutSuccess( out Reply<bool> opt )
            ? Reply<bool>.Success( true )
            : opt;

    static TimeSpan GetRefreshInterval( IConfiguration config ) =>
        config.GetSection( "Ordering:OrderPendingCancelService" ).GetValue<TimeSpan>( "RefreshInterval" );
}