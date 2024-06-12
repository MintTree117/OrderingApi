using OrderingDomain.Account;
using OrderingDomain.Optionals;

namespace OrderingInfrastructure.Features.Account.Repositories;

public interface ISessionRepository : IEfCoreRepository
{
    public Task<Replies<UserSession>> GetSessionsByUserId( string userId );
    public Task<Reply<bool>> DeleteSession( Guid sessionId );
}