using Aurora.Coinly.Infrastructure.Database;
using Aurora.Coinly.Infrastructure.Time;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aurora.Coinly.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database connection
        var connectionString = configuration.GetConnectionString("AuroraCoinlyConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseNpgsql(
                    connectionString,
                    x => x.MigrationsHistoryTable(HistoryRepository.DefaultTableName, ApplicationDbContext.DEFAULT_SCHEMA))
                .UseSnakeCaseNamingConvention());

        // IUnitOfWork implementation
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // DateTime services
        services.TryAddSingleton<IDateTimeService, DateTimeService>();

        return services;
    }
}