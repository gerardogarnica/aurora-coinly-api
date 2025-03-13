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
        string? notes,
        DateTime createdOnUtc)
    {
        var budget = new Budget
        {
            CategoryId = category.Id,
            AmountLimit = amountLimit,
            Period = period,
            Notes = notes,
            Status = BudgetStatus.Draft,
            CreatedOnUtc = createdOnUtc
        };

        return budget;
    }

    public Result<Budget> Update(
        Money amountLimit,
        DateRange period,
        string? notes,
        DateTime updatedOnUtc)
    {
        if (Status is BudgetStatus.Closed)
        {
            return Result.Fail<Budget>(BudgetErrors.IsClosed);
        }

        AmountLimit = amountLimit;
        Period = period;
        Notes = notes;
        UpdatedOnUtc = updatedOnUtc;

        return this;
    }

    public Result<Budget> Close(DateTime closedOnUtc)
    {
        if (Status is BudgetStatus.Closed)
        {
            return Result.Fail<Budget>(BudgetErrors.IsClosed);
        }

        Status = BudgetStatus.Closed;
        ClosedOnUtc = closedOnUtc;

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

    public Result<Budget> RemoveTransaction(Transaction transaction)
    {
        if (Status is BudgetStatus.Closed)
        {
            return Result.Fail<Budget>(BudgetErrors.IsClosed);
        }

        if (!_transactions.Any(t => t.TransactionId == transaction.Id))
        {
            return Result.Fail<Budget>(BudgetErrors.TransactionNotBelongs);
        }

        if (transaction.IsPaid)
        {
            return Result.Fail<Budget>(TransactionErrors.AlreadyPaid);
        }

        _transactions.Remove(_transactions.First(t => t.TransactionId == transaction.Id));

        AddDomainEvent(new BudgetUpdatedEvent(this));

        return this;
    }
}