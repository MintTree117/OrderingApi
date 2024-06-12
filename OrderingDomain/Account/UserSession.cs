using OrderingDomain._Common;

namespace OrderingDomain.Account;

public sealed class UserSession : IEntity
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
    public DateTime LastActive { get; set; }
    public string? IpAddress { get; set; }
}