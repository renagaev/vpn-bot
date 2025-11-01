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
        if (client == null)
        {
            return;
        }

        client.IsSubscribed = false;
        await dbContext.SaveChangesAsync(cancellationToken);

        var clientSettings = await xuiService.GetClient(client.PanelId, cancellationToken);
        if (clientSettings == null)
        {
            return;
        }

        if (clientSettings.Enable)
        {
            clientSettings.Enable = false;
            await xuiService.UpdateClient(clientSettings, cancellationToken);
            await telegramService.SendMessage(client.ChatId!.Value, "Пока :(", cancellationToken);
        }
    }
}