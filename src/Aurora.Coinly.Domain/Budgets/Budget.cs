using Aurora.Coinly.Domain.Categories;
using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Domain.Budgets;

public sealed class Budget : BaseEntity
{
    private readonly List<BudgetPeriod> _periods = [];

    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }
    public int Year { get; private set; }
    public BudgetFrequency Frequency { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? UpdatedOnUtc { get; private set; }
    public Category Category { get; init; } = null!;
    public IReadOnlyCollection<BudgetPeriod> Periods => _periods.AsReadOnly();

    private Budget() : base(Guid.NewGuid())
    {
        CategoryId = Guid.Empty;
    }

    public static Budget Create(
        Guid userId,
        Category category,
        int year,
        BudgetFrequency frequency,
        Money limit,
        DateTime createdOnUtc,
        BudgetPeriodService periodService)
    {
        var budget = new Budget
        {
            UserId = userId,
            CategoryId = category.Id,
            Year = year,
            Frequency = frequency,
            CreatedOnUtc = createdOnUtc
        };

        var periods = periodService.GeneratePeriods(frequency, budget.Year);
        foreach (var period in periods)
        {
            var periodLimit = new Money(limit.Amount, limit.Currency);

            var budgetPeriod = BudgetPeriod.Create(
                budget,
                period,
                periodLimit,
                createdOnUtc);

            budget._periods.Add(budgetPeriod);
        }

        return budget;
    }

    public Result<Budget> UpdateLimit(
        Guid periodId,
        Money limit,
        DateTime updatedOnUtc)
    {
        var period = _periods.FirstOrDefault(p => p.Id == periodId);
        if (period is null)
        {
            return Result.Fail<Budget>(BudgetErrors.PeriodNotFound);
        }

        period.Update(limit, updatedOnUtc);

        return this;
    }

    public Result<BudgetTransaction> AssignTransaction(Transaction transaction)
    {
        if (transaction.CategoryId != CategoryId)
        {
            return Result.Fail<BudgetTransaction>(BudgetErrors.TransactionCategoryMismatch);
        }

        if (!transaction.IsPaid)
        {
            return Result.Fail<BudgetTransaction>(BudgetErrors.TransactionNotPaid);
        }

        var period = _periods.FirstOrDefault(p => p.Period.Contains(transaction.PaymentDate!.Value));
        if (period is null)
        {
            return Result.Fail<BudgetTransaction>(BudgetErrors.TransactionPaymentDateOutOfRange);
        }

        return period.AssignTransaction(transaction);
    }

    public Result<Budget> RemoveTransaction(Transaction transaction, DateOnly originalPaymentDate)
    {
        var period = _periods.FirstOrDefault(p => p.Period.Contains(originalPaymentDate));
        if (period is null)
        {
            return Result.Fail<Budget>(BudgetErrors.TransactionPaymentDateOutOfRange);
        }

        if (!period.Transactions.Any(t => t.TransactionId == transaction.Id))
        {
            return Result.Fail<Budget>(BudgetErrors.TransactionNotBelongs);
        }

        if (transaction.IsPaid)
        {
            return Result.Fail<Budget>(BudgetErrors.TransactionAlreadyIsPaid);
        }

        period.RemoveTransaction(transaction);

        return this;
    }
}