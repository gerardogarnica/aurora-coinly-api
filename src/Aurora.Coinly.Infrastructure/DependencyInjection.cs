using Aurora.Coinly.Domain.Budgets;
using Aurora.Coinly.Domain.Categories;
using Aurora.Coinly.Domain.Methods;
using Aurora.Coinly.Domain.Summary;
using Aurora.Coinly.Domain.Transactions;
using Aurora.Coinly.Domain.Users;
using Aurora.Coinly.Domain.Wallets;
using Aurora.Coinly.Infrastructure.Authentication;
using Aurora.Coinly.Infrastructure.Budgets;
using Aurora.Coinly.Infrastructure.Categories;
using Aurora.Coinly.Infrastructure.Interceptors;
using Aurora.Coinly.Infrastructure.Methods;
using Aurora.Coinly.Infrastructure.Summary;
using Aurora.Coinly.Infrastructure.Transactions;
using Aurora.Coinly.Infrastructure.Users;
using Aurora.Coinly.Infrastructure.Wallets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Aurora.Coinly.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Authentication
        services.AddAuthenticationServices(configuration);

        // Quartz configuration
        services.AddQuartz();
        services.AddQuartzHostedService(cfg => cfg.WaitForJobsToComplete = true);

        // Encryption services
        services.AddEncryptionServices(configuration);

        // Outbox interceptor
        services.TryAddSingleton<InsertOutboxMessagesInterceptor>();

        // Database connection
        var connectionString = configuration.GetConnectionString("AuroraCoinlyConnection");

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    connectionString,
                    x => x.MigrationsHistoryTable(HistoryRepository.DefaultTableName, ApplicationDbContext.DEFAULT_SCHEMA))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>()));

        // IUnitOfWork implementation
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Repository implementations
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<IMonthlySummaryRepository, MonthlySummaryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // User context implementation
        services.AddScoped<IUserContext, UserContext>();

        // Outbox pattern implementation
        services.AddOptions<OutboxOptions>().BindConfiguration("Outbox");
        services.ConfigureOptions<ConfigureProcessOutboxJob>();

        // DateTime services
        services.TryAddSingleton<IDateTimeService, DateTimeService>();

        return services;
    }

    private static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.ConfigureOptions<JwtBearerConfigureOptions>();

        services.Configure<JwtAuthOptions>(configuration.GetSection(JwtAuthOptions.SectionName));
        JwtAuthOptions jwtAuthOptions = configuration.GetSection(JwtAuthOptions.SectionName).Get<JwtAuthOptions>()!;

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenProvider, TokenProvider>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtAuthOptions.Issuer,
                    ValidAudience = jwtAuthOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthOptions.Key))
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static IServiceCollection AddEncryptionServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EncryptionOptions>(configuration.GetSection(EncryptionOptions.SectionName));

        services.AddTransient<EncryptionService>();

        return services;
    }
}