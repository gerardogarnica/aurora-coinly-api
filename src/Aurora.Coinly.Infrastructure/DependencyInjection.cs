using Aurora.Coinly.Domain.Budgets;
using Aurora.Coinly.Domain.Categories;
using Aurora.Coinly.Domain.Methods;
using Aurora.Coinly.Domain.Summary;
using Aurora.Coinly.Domain.Transactions;
using Aurora.Coinly.Domain.Wallets;
using Aurora.Coinly.Infrastructure.Budgets;
using Aurora.Coinly.Infrastructure.Categories;
using Aurora.Coinly.Infrastructure.Database;
using Aurora.Coinly.Infrastructure.Methods;
using Aurora.Coinly.Infrastructure.Summary;
using Aurora.Coinly.Infrastructure.Time;
using Aurora.Coinly.Infrastructure.Transactions;
using Aurora.Coinly.Infrastructure.Wallets;
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

        // Repository implementations
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<IMonthlySummaryRepository, MonthlySummaryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();

        // DateTime services
        services.TryAddSingleton<IDateTimeService, DateTimeService>();

        return services;
    }
}