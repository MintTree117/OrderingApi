using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;

namespace OrderingInfrastructure.Features.Account.Repositories;

public interface ISessionRepository : IEfCoreRepository
{
    public Task<Replies<UserSession>> GetSessions( string userId );
    public Task<Reply<UserSession>> GetSession( string sessionId, string userId );
    public Task<IReply> AddSession( UserSession session );
    public Task<IReply> DeleteSession( string sessionId, string userId );
}