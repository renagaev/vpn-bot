namespace Infrastructure.Implementation.Telegram;

public class TelegramSettings
{
    public string Token { get; init; }
    public long ChannelId { get; init; }
    public long AdminChatId { get; init; }
    public string GetSubscriptionCommand { get; init; } = "Дай";
}