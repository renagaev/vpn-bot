using Infrastructure.Interfaces.Telegram;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace Infrastructure.Implementation.Telegram;

public static class DependencyInjection
{
    public static IServiceCollection AddTelegramServices(this IServiceCollection services)
    {
        services.AddScoped<ITelegramService, TelegramService>();
        services.AddHostedService<TelegramHostedService>();
        services.AddSingleton<UpdateHandler>();
        
        services.AddSingleton<ITelegramBotClient, TelegramBotClient>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<TelegramSettings>>();
            return new TelegramBotClient(options.Value.Token);
        });
        return services;
    }
}