using Aurora.Coinly.Domain.Budgets;
using Aurora.Coinly.Domain.Categories;
using Aurora.Coinly.Domain.Methods;
using Aurora.Coinly.Domain.Summary;
using Aurora.Coinly.Domain.Transactions;
using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Infrastructure.Database;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options) : DbContext(options), IUnitOfWork
{
    internal const string DEFAULT_SCHEMA = "coinly";

    public DbSet<Budget> Budgets { get; set; } = null!;
    public DbSet<BudgetPeriod> BudgetPeriods { get; set; } = null!;
    public DbSet<BudgetTransaction> BudgetTransactions { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<MonthlySummary> MonthlySummaries { get; set; } = null!;
    public DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<Wallet> Wallets { get; set; } = null!;
    public DbSet<WalletHistory> WalletHistories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DEFAULT_SCHEMA);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}