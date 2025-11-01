using Infrastructure.Interfaces.XUI;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Implementation.XUI;

public static class DependencyInjection
{
    public static IServiceCollection AddXUIClient(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddHttpClient<IXUIClient, XUIClient>();
        
        return serviceCollection;
    }
}