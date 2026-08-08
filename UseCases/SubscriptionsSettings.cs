namespace UseCases;

public class SubscriptionsSettings
{
    public string Url { get; set; }
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(30);
    public string CacheKey { get; set; } = "sub";
}