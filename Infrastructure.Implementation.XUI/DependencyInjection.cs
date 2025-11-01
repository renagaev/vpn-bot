using Infrastructure.Interfaces.XUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.Implementation.XUI;

public static class DependencyInjection
{
    public static IServiceCollection AddXUIClient(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddHttpClient<IXUIClient, XUIClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptionsMonitor<XUISettings>>();
            client.BaseAddress = new Uri(options.CurrentValue.BaseUrl);
        });
        
        return serviceCollection;
    }
}