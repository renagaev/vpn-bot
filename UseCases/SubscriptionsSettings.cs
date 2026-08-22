namespace UseCases;

public class SubscriptionsSettings
{
    public List<string> Urls { get; set; } = new();
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(30);
    public string Title { get; set; } = "Эчпочмак VPN";
    public int UpdateIntervalHours { get; set; } = 1;
}
