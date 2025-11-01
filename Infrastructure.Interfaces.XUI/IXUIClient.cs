namespace Infrastructure.Interfaces.XUI;

public interface IXUIClient
{
    Task CreateClient(long inboundId, ClientSettings clientSettings, CancellationToken cancellationToken);
    Task UpdateClient(long inboundId, ClientSettings clientSettings, CancellationToken cancellationToken);

    Task<ICollection<ClientSettings>> GetInboundClients(long inboundId, CancellationToken cancellationToken);
}