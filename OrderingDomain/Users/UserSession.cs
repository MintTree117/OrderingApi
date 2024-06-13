namespace OrderingDomain.Users;

public sealed class UserSession
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
    public DateTime LastActive { get; set; }
    public string? IpAddress { get; set; }
}