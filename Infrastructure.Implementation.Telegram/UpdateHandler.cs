using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using UseCases.Commands;

namespace Infrastructure.Implementation.Telegram;

internal class UpdateHandler(IServiceScopeFactory scopeFactory, IOptions<TelegramSettings> options, ILogger<UpdateHandler> logger) : IUpdateHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var settings = options.Value;
        if (update is { Type: UpdateType.ChatMember, ChatMember.NewChatMember: { IsInChat: false, User: var lefMember }, } && update.ChatMember?.Chat.Id == settings.ChannelId)
        {
            await sender.Send(new UnsubscribeClientCommand(lefMember.Id), cancellationToken);
            return;
        }
        
        if (update is { Type: UpdateType.ChatMember, ChatMember.NewChatMember: { IsInChat: true, User: var joinedMember } } && update.ChatMember?.Chat.Id == settings.ChannelId)
        {
            await sender.Send(new SaveChannelSubscriberCommand(joinedMember.Id, joinedMember.Username, $"{joinedMember.FirstName} {joinedMember.LastName}"), cancellationToken);
            return;
        }

        if (update.Message is { Type: MessageType.Text, From: var user, Text: var text } && text == settings.GetSubscriptionCommand)
        {
            await sender.Send(new SubscribeClientCommand(update.Message.Chat.Id, user.Id,
                $"{user.FirstName} {user.LastName}", user.Username), cancellationToken);
            return;
        }
        
        if (update.Message != null)
        {
            await botClient.SendMessage(update.Message.Chat.Id, ";|",replyMarkup: new ReplyKeyboardMarkup(new KeyboardButton(settings.GetSubscriptionCommand)), cancellationToken: cancellationToken);
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