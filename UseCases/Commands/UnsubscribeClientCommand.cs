using Infrastructure.Interfaces.DataAccess;
using Infrastructure.Interfaces.Telegram;
using Infrastructure.Interfaces.XUI;
using MediatR;

namespace UseCases.Commands;

public record UnsubscribeClientCommand(long TgId) : IRequest;

public class UnsubscribeClientCommandHandler(
    IDbContext dbContext,
    XUIService xuiService,
    ITelegramService telegramService) : IRequestHandler<UnsubscribeClientCommand>
{
    public async Task Handle(UnsubscribeClientCommand request, CancellationToken cancellationToken)
    {
        var client = dbContext.Users.FirstOrDefault(x => x.Id == request.TgId);
        if (client is not { IsSubscribed: true, PanelId: not null })
        {
            return;
        }

        var clientSettings = await xuiService.GetClient(client.PanelId, cancellationToken);
        if (clientSettings == null)
        {
            return;
        }

        if (clientSettings.Enable)
        {
            clientSettings.Enable = false;
            await xuiService.UpdateClient(clientSettings, cancellationToken);
            client.IsSubscribed = false;
            await dbContext.SaveChangesAsync(cancellationToken);
            await telegramService.SendMessage(client.ChatId, "До свидания :(", cancellationToken);
        }
    }
}