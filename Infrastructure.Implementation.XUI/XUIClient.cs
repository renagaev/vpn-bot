using System.Net.Http.Json;
using System.Text.Json;
using Infrastructure.Interfaces.XUI;
using Microsoft.Extensions.Options;

namespace Infrastructure.Implementation.XUI;

internal class XUIClient(HttpClient client, IOptionsSnapshot<XUISettings> options) : IXUIClient
{
    private static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions();

    private async Task Login(CancellationToken cancellationToken)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = options.Value.Username,
            ["password"] = options.Value.Password
        });
        var res = await client.PostAsync("login", content, cancellationToken);
        res.EnsureSuccessStatusCode();
    }

    public async Task CreateClient(long inboundId, ClientSettings clientSettings, CancellationToken cancellationToken)
    {
        await Login(cancellationToken);

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id"] = inboundId.ToString(),
            ["settings"] = JsonSerializer.Serialize(new ClientCollection
            {
                Clients = [clientSettings]
            }, JsonSerializerOptions)
        });

        var res = await client.PostAsync("panel/api/inbounds/addClient", content, cancellationToken);
        res.EnsureSuccessStatusCode();
    }

    public async Task UpdateClient(long inboundId, ClientSettings clientSettings, CancellationToken cancellationToken)
    {
        await Login(cancellationToken);

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id"] = inboundId.ToString(),
            ["settings"] = JsonSerializer.Serialize(new ClientCollection
            {
                Clients = [clientSettings]
            }, JsonSerializerOptions)
        });

        var res = await client.PostAsync($"/panel/api/inbounds/updateClient/{clientSettings.Id}", content, cancellationToken);
        res.EnsureSuccessStatusCode();
    }

    public async Task<ICollection<ClientSettings>> GetInboundClients(long inboundId, CancellationToken cancellationToken)
    {
        await Login(cancellationToken);
        var res = await client.GetAsync("/panel/api/inbounds/get/1", cancellationToken);
        res.EnsureSuccessStatusCode();
        var resObj = await res.Content.ReadFromJsonAsync<GetInboundResponse>(JsonSerializerOptions, cancellationToken: cancellationToken);
        var clients = JsonSerializer.Deserialize<ClientCollection>(resObj.Obj.Settings, JsonSerializerOptions);
        return clients.Clients;
    }
}