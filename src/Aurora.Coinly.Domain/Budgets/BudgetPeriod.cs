using Aurora.Coinly.Domain.Categories;
using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Domain.Budgets;

public class BudgetPeriod
{
    private readonly List<BudgetTransaction> _transactions = [];

    public Guid Id { get; private set; }
    public Guid BudgetId { get; private set; }
    public DateRange Period { get; private set; }
    public Money Limit { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? UpdatedOnUtc { get; private set; }
    public IReadOnlyCollection<BudgetTransaction> Transactions => _transactions.AsReadOnly();

    private BudgetPeriod()
    {
        Id = Guid.NewGuid();
    }

    internal static BudgetPeriod Create(
        Budget budget,
        DateRange period,
        Money limit,
        DateTime createdOnUtc)
    {
        return new BudgetPeriod
        {
            BudgetId = budget.Id,
            Period = period,
            Limit = limit,
            CreatedOnUtc = createdOnUtc
        };
    }

    internal void Update(Money limit, DateTime updatedOnUtc)
    {
        Limit = limit;
        UpdatedOnUtc = updatedOnUtc;
    }

    public bool IsExceeded()
    {
        return GetSpentAmount() > Limit;
    }

    public Money GetSpentAmount()
    {
        return Transactions
            .Select(t => t.Amount)
            .Aggregate(Money.Zero(Currency.Usd), (acc, amount) => acc + amount);
    }

    internal BudgetTransaction AssignTransaction(Transaction transaction)
    {
        var budgetTransaction = BudgetTransaction.Create(this, transaction);

        _transactions.Add(budgetTransaction);

        return budgetTransaction;
    }

    internal void RemoveTransaction(Transaction transaction)
    {
        _transactions.Remove(_transactions.First(t => t.TransactionId == transaction.Id));
    }
}