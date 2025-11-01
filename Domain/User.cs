namespace Domain;

public class User
{
    public long Id { get; init; }
    public long ChatId { get; init; }
    public string Username { get; init; }
    public string Title { get; init; }
    public bool IsSubscribed { get; set; }
    public string? PanelId { get; set; }
    public string? SubId { get; set; }
}