namespace Domain;

public class UserSubscriptionAccess
{
    public long Id { get; init; }
    public required long UserId { get; init; }
    public required string UserAgent { get; init; }
    public required string Hwid { get; init; }
    public DateTime LastSeenAt { get; set; }
}
