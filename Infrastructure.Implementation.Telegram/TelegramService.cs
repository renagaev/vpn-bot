using Infrastructure.Interfaces.Telegram;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Infrastructure.Implementation.Telegram;

public class TelegramService(ITelegramBotClient botClient, IOptionsSnapshot<TelegramSettings> settings)
    : ITelegramService
{
    public async Task<bool> IsSubscriber(long userId, CancellationToken cancellationToken)
    {
        var res = await botClient.GetChatMember(settings.Value.ChannelId, userId, cancellationToken);
        return res.IsInChat;
    }

    public async Task SendMessage(long chatId, string message, CancellationToken cancellationToken)
    {
        try
        {
            await botClient.SendMessage(chatId, message, ParseMode.Markdown, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            // ignored
        }
    }
}