using Domain;
using Infrastructure.Interfaces.DataAccess;
using MediatR;

namespace UseCases.Commands;

public record SaveChannelSubscriberCommand(long TgId, string Username, string Title): IRequest;

public class SaveChannelSubscriberHandler(IDbContext dbContext, XUIService xuiService) : IRequestHandler<SaveChannelSubscriberCommand>
{
    public async Task Handle(SaveChannelSubscriberCommand request, CancellationToken cancellationToken)
    {
        var user = dbContext.Users.FirstOrDefault(x => x.Id == request.TgId);
        if (user is not null)
        {
            user.IsSubscribed = true;
            if (user.PanelId is not null)
            {
                var client = await xuiService.GetClient(user.PanelId, cancellationToken);
                if (client is not null)
                {
                    client.Enable = true;
                    await xuiService.UpdateClient(client, cancellationToken);
                }
            }
        }
        else
        {
            dbContext.Users.Add(new User
            {
                Id = request.TgId,
                Username = request.Username,
                Title = request.Title,
                IsSubscribed = true
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

    }
}