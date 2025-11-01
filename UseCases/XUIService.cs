using Infrastructure.Interfaces.XUI;
using Microsoft.Extensions.Options;

namespace UseCases;

public class XUIService(IXUIClient xuiClient, IOptionsSnapshot<VpnSettings> vpnSettings)
{
    public async Task<ClientSettings?> GetClient(string panelId, CancellationToken cancellationToken)
    {
        var allClients = await xuiClient.GetInboundClients(vpnSettings.Value.InboundId, cancellationToken);
        return allClients.FirstOrDefault(x => x.Id == panelId);
    }

    public async Task UpdateClient(ClientSettings clientSettings, CancellationToken cancellationToken)
    {
        await xuiClient.UpdateClient(vpnSettings.Value.InboundId, clientSettings, cancellationToken);
    }

    public async Task CreateClient(ClientSettings clientSettings, CancellationToken cancellationToken)
    {
        await xuiClient.CreateClient(vpnSettings.Value.InboundId, clientSettings, cancellationToken);
    }
}