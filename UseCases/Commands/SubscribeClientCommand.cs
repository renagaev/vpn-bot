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
        var isSubscriber = await telegramService.IsSubscriber(request.TgId, cancellationToken);
        if (!isSubscriber)
        {
            await telegramService.SendMessage(request.ChatId, "Ты не подписчик", cancellationToken);
            return;
        }

        var inboundId = vpnSettings.Value.InboundId;

        var user = await GetUser(request, cancellationToken);

        if (user.PanelId != null)
        {
            var client = await xuiService.GetClient(user.PanelId, cancellationToken);
            if (client != null)
            {
                client.Enable = true;
                await xuiService.UpdateClient(client, cancellationToken);
                await ReturnLink(request.ChatId, client.SubId, cancellationToken);
                return;
            }
        }

        var panelId = Guid.NewGuid().ToString();
        var subId = Guid.NewGuid().ToString();

        var settings = new ClientSettings
        {
            Id = panelId,
            TgId = request.TgId.ToString(),
            Flow = vpnSettings.Value.Flow,
            Comment = $"{request.UserTitle} | @{request.Username}",
            Email = panelId,
            SubId = subId
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
        await telegramService.SendMessage(chatId, $"Держи подписку: {link}", cancellationToken);
    }

    private async Task<User> GetUser(SubscribeClientCommand request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == request.TgId, cancellationToken);
        if (user != null) return user;
        user = new User
        {
            Id = request.TgId,
            Username = request.Username,
            Title = request.UserTitle,
            IsSubscribed = true,
            ChatId = request.ChatId
        };
        dbContext.Users.Add(user);

        return user;
    }
}