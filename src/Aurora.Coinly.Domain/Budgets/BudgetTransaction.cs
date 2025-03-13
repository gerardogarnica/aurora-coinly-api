using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Domain.Budgets;

public sealed class BudgetTransaction
{
    public Guid Id { get; private set; }
    public Guid BudgetId { get; private set; }
    public Guid TransactionId { get; private set; }
    public string Description { get; private set; }
    public Money Amount { get; private set; }
    public DateOnly TransactionDate { get; private set; }
    public Budget Budget { get; init; } = null!;

    private BudgetTransaction()
    {
        Id = Guid.NewGuid();
    }

    internal static BudgetTransaction Create(
        Budget budget,
        Transaction transaction)
    {
        return new BudgetTransaction
        {
            BudgetId = budget.Id,
            TransactionId = transaction.Id,
            Description = transaction.Description,
            Amount = transaction.Amount,
            TransactionDate = transaction.TransactionDate
        };
    }
}