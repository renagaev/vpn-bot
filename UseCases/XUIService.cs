using Infrastructure.Interfaces.XUI;
using Microsoft.Extensions.Options;

namespace UseCases;

public class XUIService(IXUIClient xuiClient, IOptionsSnapshot<VpnSettings> vpnSettings)
{
    public async Task<ClientSettings?> GetClient(string panelId, CancellationToken cancellationToken)
    {
        var allClients = await xuiClient.GetInboundClients(vpnSettings.Value.InboundIds[0], cancellationToken);
        return allClients.FirstOrDefault(x => x.Id == panelId);
    }

    public async Task UpdateClient(ClientSettings clientSettings, CancellationToken cancellationToken)
    {
        foreach (var inboundId in vpnSettings.Value.InboundIds)
        {
            var clients = await xuiClient.GetInboundClients(inboundId, cancellationToken);
            var client = clients.FirstOrDefault(x => x.Id == clientSettings.Id);
            if (client == null)
                return;
            client.Enable = clientSettings.Enable;
            await xuiClient.UpdateClient(inboundId, client, cancellationToken);
        }
    }

    public async Task CreateClient(ClientSettings clientSettings, CancellationToken cancellationToken)
    {
        foreach (var inboundId in vpnSettings.Value.InboundIds)
        {
            var settings = clientSettings with { Email = Guid.NewGuid().ToString() };
            await xuiClient.CreateClient(inboundId, settings, cancellationToken);
        }
    }
}