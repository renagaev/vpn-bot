using System.Net;
using Infrastructure.Interfaces.HappSpoofer;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Implementation.HappSpoofer;

public static class DependencyInjection
{
    public static IServiceCollection AddHappSpoofer(this IServiceCollection services)
    {
        services.AddHttpClient<HappSpoofer>().ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        });
        services.AddSingleton<IHappSpoofer, HappSpoofer>();
        return services;
    }
}