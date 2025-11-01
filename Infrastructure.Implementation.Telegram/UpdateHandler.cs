using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using UseCases.Commands;

namespace Infrastructure.Implementation.Telegram;

internal class UpdateHandler(IServiceScopeFactory scopeFactory, IOptions<TelegramSettings> options, ILogger<UpdateHandler> logger) : IUpdateHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var settings = options.Value;
        if (update is { Type: UpdateType.ChatMember, ChatMember.NewChatMember: { IsInChat: false, User: var user } })
        {
            await sender.Send(new UnsubscribeClientCommand(user.Id), cancellationToken);
            return;
        }
        
        var message = update.Message;
        if (message is not { Chat.Id: var chatId, From.Id: var userId, Text: var text })
        {
            return;
        }   
    }



    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Error during update processing");
        return Task.CompletedTask;
    }
}