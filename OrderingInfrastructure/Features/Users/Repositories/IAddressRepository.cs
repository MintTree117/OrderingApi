using OrderingDomain.ReplyTypes;
using OrderingDomain.Users;

namespace OrderingInfrastructure.Features.Users.Repositories;

public interface IAddressRepository : IEfCoreRepository
{
    public Task<Reply<UserAddress>> GetAddress( Guid addressId );
    public Task<Replies<UserAddress>> GetAllAddresses( string userId );
    public Task<Reply<PagedResult<UserAddress>>> GetPagedAddresses( string userId, int page, int pageSize );
    public Task<IReply> AddAddress( UserAddress address );
    public Task<IReply> UpdateAddress( UserAddress address );
    public Task<IReply> DeleteAddress( UserAddress address );
}