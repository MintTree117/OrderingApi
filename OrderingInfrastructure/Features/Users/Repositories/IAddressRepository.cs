using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;

namespace OrderingInfrastructure.Features.Users.Repositories;

public interface IAddressRepository : IEfCoreRepository
{
    public Task<Reply<UserAddress>> GetById( Guid addressId );
    public Task<Replies<UserAddress>> GetAllByUserId( string userId );
    public Task<Reply<PagedResult<UserAddress>>> GetAllByUserIdPaged( string userId, int page, int pageSize );
    public Task<Reply<bool>> Add( UserAddress address );
    public Task<Reply<bool>> Delete( UserAddress address );
}