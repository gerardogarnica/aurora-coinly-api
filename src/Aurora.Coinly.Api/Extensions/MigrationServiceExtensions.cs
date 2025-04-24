using Aurora.Coinly.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Aurora.Coinly.Api.Extensions;

internal static class MigrationServiceExtensions
{
    internal static void ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Database.Migrate();
    }
}