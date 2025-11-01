namespace Infrastructure.Interfaces.XUI;

public class ClientSettings
{
    public string Id { get; init; }
    public string Flow { get; init; }
    public string Email { get; init; }
    public long ExpiryTime { get; init; }
    public long TotalGB { get; init; }
    public bool Enable { get; set; }
    public string SubId { get; init; }
    public string Comment { get; init; }
    public long Reset { get; init; }
}