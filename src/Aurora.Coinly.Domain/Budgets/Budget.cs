using Aurora.Coinly.Domain.Categories;
using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Domain.Budgets;

public sealed class Budget : BaseEntity
{
    private readonly List<BudgetTransaction> _transactions = [];

    public Guid CategoryId { get; private set; }
    public Money AmountLimit { get; private set; }
    public DateRange Period { get; private set; }
    public string? Notes { get; private set; }
    public BudgetStatus Status { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? UpdatedOnUtc { get; private set; }
    public DateTime? ClosedOnUtc { get; private set; }
    public Category Category { get; init; } = null!;
    public IReadOnlyCollection<BudgetTransaction> Transactions => _transactions.AsReadOnly();

    private Budget() : base(Guid.NewGuid())
    {
        CategoryId = Guid.Empty;
        AmountLimit = Money.Zero();
    }

    public static Budget Create(
        Category category,
        Money amountLimit,
        DateRange period,
        string? notes)
    {
        var budget = new Budget
        {
            CategoryId = category.Id,
            AmountLimit = amountLimit,
            Period = period,
            Notes = notes,
            Status = BudgetStatus.Draft,
            CreatedOnUtc = DateTime.UtcNow
        };

        return budget;
    }

    public Result<Budget> Update(
        Money amountLimit,
        DateRange period,
        string? notes)
    {
        if (Status is BudgetStatus.Closed)
        {
            return Result.Fail<Budget>(BudgetErrors.IsClosed);
        }

        AmountLimit = amountLimit;
        Period = period;
        Notes = notes;

        return this;
    }

    public Result<Budget> Close()
    {
        if (Status is BudgetStatus.Closed)
        {
            return Result.Fail<Budget>(BudgetErrors.IsClosed);
        }

        Status = BudgetStatus.Closed;
        ClosedOnUtc = DateTime.UtcNow;

        return this;
    }

    public bool IsExceeded()
    {
        return GetSpentAmount() > AmountLimit;
    }

    public Money GetSpentAmount()
    {
        return Transactions
            .Select(t => t.Amount)
            .Aggregate(Money.Zero(), (acc, amount) => acc + amount);
    }

    public Result<Budget> AssignTransaction(Transaction transaction)
    {
        if (Status is BudgetStatus.Closed)
        {
            return Result.Fail<Budget>(BudgetErrors.IsClosed);
        }

        if (transaction.CategoryId != CategoryId)
        {
            return Result.Fail<Budget>(BudgetErrors.TransactionCategoryMismatch);
        }

        if (!Period.Contains(transaction.PaymentDate!.Value))
        {
            return Result.Fail<Budget>(BudgetErrors.TransactionPaymentDateOutOfRange);
        }

        if (!transaction.IsPaid)
        {
            return Result.Fail<Budget>(BudgetErrors.TransactionNotPaid);
        }

        Status = BudgetStatus.Active;
        _transactions.Add(BudgetTransaction.Create(this, transaction));

        AddDomainEvent(new BudgetUpdatedEvent(this));

        return this;
    }
}