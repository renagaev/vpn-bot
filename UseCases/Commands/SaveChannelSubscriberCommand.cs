using Domain;
using Infrastructure.Interfaces.DataAccess;
using MediatR;

namespace UseCases.Commands;

public record SaveChannelSubscriberCommand(long TgId, string Username, string Title): IRequest;

public class SaveChannelSubscriberHandler(IDbContext dbContext) : IRequestHandler<SaveChannelSubscriberCommand>
{
    public async Task Handle(SaveChannelSubscriberCommand request, CancellationToken cancellationToken)
    {
        var user = dbContext.Users.FirstOrDefault(x => x.Id == request.TgId);
        if (user is not null)
        {
            return;
        }

        dbContext.Users.Add(new User
        {
            Id = request.TgId,
            Username = request.Username,
            Title = request.Title,
            IsSubscribed = false
        });
    }
}