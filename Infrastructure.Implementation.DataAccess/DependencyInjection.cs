using Infrastructure.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Infrastructure.Implementation.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DBConnection");
        var datasource = new NpgsqlDataSourceBuilder(connectionString)
            .EnableDynamicJson()
            .Build();

        services.AddDbContext<IDbContext, AppDbContext>((provider, options) =>
        {
            options.UseNpgsql(datasource)
                .UseSnakeCaseNamingConvention();
        });

        return services;
    } 
}