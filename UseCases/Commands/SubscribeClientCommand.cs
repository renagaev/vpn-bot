using Domain;
using Infrastructure.Interfaces.DataAccess;
using Infrastructure.Interfaces.Telegram;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace UseCases.Commands;

public record SubscribeClientCommand(long ChatId, long TgId, string UserTitle, string Username) : IRequest;

public class SubscribeClientCommandHandler(
    IDbContext dbContext,
    ITelegramService telegramService,
    IOptionsSnapshot<VpnSettings> vpnSettings) : IRequestHandler<SubscribeClientCommand>
{
    public async Task Handle(SubscribeClientCommand request, CancellationToken cancellationToken)
    {
        var user = await GetUser(request, cancellationToken);
        if (!user.IsSubscribed)
        {
            await telegramService.SendMessage(request.ChatId, "Не дам, ты не подписчик :|", cancellationToken);
            return;
        }
        if (user.PanelId != null)
        {
            await ReturnLink(request.ChatId, user.SubId!, cancellationToken);
            return;
        }

        var panelId = Guid.NewGuid().ToString();
        var subId = Guid.NewGuid().ToString();
        user.PanelId = panelId;
        user.SubId = subId;
        await dbContext.SaveChangesAsync(cancellationToken);
        await ReturnLink(request.ChatId, subId, cancellationToken);
    }

    private async Task ReturnLink(long chatId, string subId, CancellationToken cancellationToken)
    {
        var link = string.Format(vpnSettings.Value.SubLinkTemplate, subId);
        await telegramService.SendMessage(chatId, "Скачай приложение Hiddify/Happ/v2raytun или Streisand(iOS), скопируй эту ссылку, нажми 'Новый профиль' и выбери 'Из буфера обмена'", cancellationToken);
        await telegramService.SendMessage(chatId, $"`{link}`", cancellationToken);
    }

    private async Task<User> GetUser(SubscribeClientCommand request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == request.TgId, cancellationToken);
        if (user != null)
        {
            user.ChatId = request.ChatId;
            await dbContext.SaveChangesAsync(cancellationToken);
            return user;
        }
        
        var isSubscriber = await telegramService.IsSubscriber(request.TgId, cancellationToken);
        user = new User
        {
            Id = request.TgId,
            Username = request.Username,
            Title = request.UserTitle,
            IsSubscribed = isSubscriber,
            ChatId = request.ChatId
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }
}