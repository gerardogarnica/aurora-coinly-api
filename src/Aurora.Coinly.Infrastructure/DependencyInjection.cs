using Aurora.Coinly.Infrastructure.Authentication;
using Aurora.Coinly.Infrastructure.Interceptors;
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

        // Encryption services
        services.AddEncryptionServices(configuration);

        // Quartz configuration
        services.AddQuartzServices();

        // Outbox interceptor
        services.AddEntityFrameworkCoreInterceptors();

        // Database configuration
        services.AddDatabaseConfiguration(configuration);

        // User context implementation
        services.AddUserContext();

        // Outbox pattern implementation
        services.AddOutboxPatternImplementation();

        // DateTime services
        services.AddDateTimeServices();

        return services;
    }

    private static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

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
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
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

    private static IServiceCollection AddQuartzServices(this IServiceCollection services)
    {
        services.AddQuartz();
        services.AddQuartzHostedService(cfg => cfg.WaitForJobsToComplete = true);

        return services;
    }
    private static IServiceCollection AddEntityFrameworkCoreInterceptors(this IServiceCollection services)
    {
        services.TryAddSingleton<InsertOutboxMessagesInterceptor>();
        return services;
    }

    private static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
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

        return services;
    }

    private static IServiceCollection AddUserContext(this IServiceCollection services)
    {
        services.AddScoped<IUserContext, UserContext>();
        return services;
    }

    private static IServiceCollection AddOutboxPatternImplementation(this IServiceCollection services)
    {
        services.AddOptions<OutboxOptions>().BindConfiguration("Outbox");
        services.ConfigureOptions<ConfigureProcessOutboxJob>();

        return services;
    }

    private static IServiceCollection AddDateTimeServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IDateTimeService, DateTimeService>();
        return services;
    }
}