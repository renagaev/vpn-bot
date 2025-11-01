namespace Infrastructure.Interfaces.Telegram;

public interface ITelegramService
{
    Task<bool> IsSubscriber(long userId, CancellationToken cancellationToken);
    Task SendMessage(long chatId, string message, CancellationToken cancellationToken);
}