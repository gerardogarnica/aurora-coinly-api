namespace Aurora.Coinly.Infrastructure.Database;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options) : DbContext(options), IUnitOfWork
{
    internal const string DEFAULT_SCHEMA = "coinly";
}