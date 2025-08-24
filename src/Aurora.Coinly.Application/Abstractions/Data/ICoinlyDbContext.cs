namespace Aurora.Coinly.Application.Abstractions.Data;

public interface ICoinlyDbContext
{
    DbSet<Budget> Budgets { get; }
    DbSet<BudgetPeriod> BudgetPeriods { get; }
    DbSet<BudgetTransaction> BudgetTransactions { get; }
    DbSet<Category> Categories { get; }
    DbSet<MonthlySummary> MonthlySummaries { get; }
    DbSet<PaymentMethod> PaymentMethods { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<User> Users { get; }
    DbSet<UserToken> UserTokens { get; }
    DbSet<Wallet> Wallets { get; }
    DbSet<WalletHistory> WalletHistories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}