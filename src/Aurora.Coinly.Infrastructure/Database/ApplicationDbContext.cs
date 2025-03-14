using Aurora.Coinly.Domain.Categories;
using Aurora.Coinly.Domain.Methods;

namespace Aurora.Coinly.Infrastructure.Database;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options) : DbContext(options), IUnitOfWork
{
    internal const string DEFAULT_SCHEMA = "coinly";

    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;
}