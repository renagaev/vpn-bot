namespace Domain;

public class User
{
    public required long Id { get; init; }
    public long? ChatId { get; set; }
    public required string? Username { get; init; }
    public required string Title { get; init; }
    public bool IsSubscribed { get; set; }
    public string? PanelId { get; set; }
    public string? SubId { get; set; }
}