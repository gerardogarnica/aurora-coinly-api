using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Domain.Budgets;

public sealed class BudgetTransaction
{
    public Guid Id { get; private set; }
    public Guid BudgetPeriodId { get; private set; }
    public Guid TransactionId { get; private set; }
    public string Description { get; private set; }
    public DateOnly TransactionDate { get; private set; }
    public Money Amount { get; private set; }

    private BudgetTransaction()
    {
        Id = Guid.NewGuid();
    }

    internal static BudgetTransaction Create(
        BudgetPeriod period,
        Transaction transaction)
    {
        return new BudgetTransaction
        {
            BudgetPeriodId = period.Id,
            TransactionId = transaction.Id,
            Description = transaction.Description,
            TransactionDate = transaction.PaymentDate!.Value,
            Amount = transaction.Amount
        };
    }
}