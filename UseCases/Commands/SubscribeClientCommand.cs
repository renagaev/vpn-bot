using Domain;
using Infrastructure.Interfaces.DataAccess;
using Infrastructure.Interfaces.Telegram;
using Infrastructure.Interfaces.XUI;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace UseCases.Commands;

public record SubscribeClientCommand(long ChatId, long TgId, string UserTitle, string Username) : IRequest;

public class SubscribeClientCommandHandler(
    IDbContext dbContext,
    XUIService xuiService,
    ITelegramService telegramService,
    IOptionsSnapshot<VpnSettings> vpnSettings) : IRequestHandler<SubscribeClientCommand>
{
    public async Task Handle(SubscribeClientCommand request, CancellationToken cancellationToken)
    {
        var user = await GetUser(request, cancellationToken);
        if (!user.IsSubscribed)
        {
            await telegramService.SendMessage(request.ChatId, "Не дам", cancellationToken);
            return;
        }
        if (user.PanelId != null)
        {
            await ReturnLink(request.ChatId, user.SubId!, cancellationToken);
            return;
        }

        var panelId = Guid.NewGuid().ToString();
        var subId = Guid.NewGuid().ToString();

        var settings = new ClientSettings
        {
            Id = panelId,
            Flow = vpnSettings.Value.Flow,
            Comment = $"{request.UserTitle} | @{request.Username}",
            Email = panelId,
            SubId = subId,
            Enable = true
        };

        await xuiService.CreateClient(settings, cancellationToken);
        user.PanelId = panelId;
        user.SubId = subId;
        await dbContext.SaveChangesAsync(cancellationToken);
        await ReturnLink(request.ChatId, subId, cancellationToken);
    }

    private async Task ReturnLink(long chatId, string subId, CancellationToken cancellationToken)
    {
        var link = string.Format(vpnSettings.Value.SubLinkTemplate, subId);
        await telegramService.SendMessage(chatId, $"На: `{link}`\nСкачай приложение `Hiddify`, скопируй эту ссылку, нажми 'Новый профиль' и выбери 'Из буфера обмена'", cancellationToken);
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