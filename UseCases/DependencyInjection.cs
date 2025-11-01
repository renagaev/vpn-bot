using Microsoft.Extensions.DependencyInjection;
using UseCases.Commands;

namespace UseCases;

public static class DependencyInjection
{
    public static IServiceCollection AddUseCases(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<XUIService>();
        serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SubscribeClientCommand>());
        return serviceCollection;
    }
}