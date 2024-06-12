using Microsoft.Extensions.Logging;

namespace OrderingInfrastructure;

internal abstract class InfrastructureService<TService>( ILogger<TService> logger )
{
    protected readonly ILogger<TService> Logger = logger;
}