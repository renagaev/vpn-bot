using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace Infrastructure.Implementation.Telegram;

internal class TelegramHostedService(UpdateHandler updateHandler, ITelegramBotClient client): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions { DropPendingUpdates = true, AllowedUpdates = [UpdateType.ChatMember, UpdateType.Message, UpdateType.MyChatMember] };
        await client.ReceiveAsync(updateHandler, receiverOptions, stoppingToken);
    }
}