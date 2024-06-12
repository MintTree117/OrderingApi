using OrderingDomain.Orders;
using OrderingDomain.ReplyTypes;
using OrderingInfrastructure.Http;

namespace OrderingApplication.Features.Ordering.Services;

internal sealed class OrderLocationCache : IDisposable
{
    readonly Timer _timer;
    readonly IHttpService _http;
    readonly string _updateUrl;
    internal IEnumerable<OrderLocation> Locations => _locations;
    internal IReadOnlyDictionary<Guid, OrderLocation> LocationsById => _locationsById;

    List<OrderLocation> _locations = [];
    Dictionary<Guid, OrderLocation> _locationsById = [];
    
    public OrderLocationCache( IConfiguration configuration, IHttpService httpService )
    {
        _http = httpService;
        TimeSpan refreshIntervalHours = GetRefreshInterval( configuration );
        _updateUrl = GetUpdateUrl( configuration );
        // ReSharper disable once AsyncVoidLambda
        _timer = new Timer( async _ => await Refresh(), null, TimeSpan.Zero, refreshIntervalHours );
    }
    
    async Task Refresh()
    {
        Dictionary<Guid, OrderLocation> cache = [];

        var refreshSucceeded = (await _http.TryGetObjRequest<List<OrderLocation>>( _updateUrl ))
            .OutSuccess( out Reply<List<OrderLocation>> reply );
        if (!refreshSucceeded) return;

        _locations = reply.Data;
        _locationsById = [];

        foreach ( OrderLocation l in _locations )
            _locationsById.Add( l.Id, l );
    }
    static TimeSpan GetRefreshInterval( IConfiguration config ) =>
        config.GetSection( "Ordering:OrderLocationCache" ).GetValue<TimeSpan>( "RefreshInterval" );
    static string GetUpdateUrl( IConfiguration config ) =>
        config.GetSection( "Ordering:OrderLocationCache" )["OrderingLocationApiUrl"] ??
        throw new Exception( $"Failed to retrieve {nameof( _updateUrl )} from config in order location cache background service." );
    public void Dispose()
    {
        _timer.Dispose();
    }
}