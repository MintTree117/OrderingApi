using OrderingDomain.Optionals;

namespace OrderingInfrastructure.Features;

public interface IEfCoreRepository
{
    public Task<Reply<bool>> SaveAsync();
}