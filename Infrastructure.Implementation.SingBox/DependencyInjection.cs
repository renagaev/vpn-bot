using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Implementation.SingBox;

public static class SingBoxServiceCollectionExtensions
{
    public static IServiceCollection AddXrayToSingBoxConverter(this IServiceCollection services)
    {
        services.AddSingleton<IXrayToSingBoxConverter, XrayToSingBoxConverter>();
        return services;
    }
}
