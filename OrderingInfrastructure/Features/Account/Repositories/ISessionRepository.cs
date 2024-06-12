using OrderingDomain.Account;
using OrderingDomain.ReplyTypes;

namespace OrderingInfrastructure.Features.Account.Repositories;

public interface ISessionRepository : IEfCoreRepository
{
    public Task<Replies<UserSession>> GetSessionsByUserId( string userId );
    public Task<Reply<bool>> DeleteSession( Guid sessionId );
}