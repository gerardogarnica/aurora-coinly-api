using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Domain.Summary;

public sealed class MonthlySummary : BaseEntity
{
    public int Year { get; private set; }
    public int Month { get; private set; }
    public Currency Currency { get; private set; }
    public decimal TotalIncome { get; private set; }
    public decimal TotalExpense { get; private set; }
    public decimal Balance => TotalIncome - TotalExpense;
    public decimal Savings { get; private set; }

    private MonthlySummary() : base(Guid.NewGuid()) { }

    public static MonthlySummary Create(
        int year,
        int month,
        Currency currency)
    {
        var summary = new MonthlySummary
        {
            Year = year,
            Month = month,
            Currency = currency,
            TotalIncome = decimal.Zero,
            TotalExpense = decimal.Zero,
            Savings = decimal.Zero
        };

        return summary;
    }

    public Result<MonthlySummary> ApplyTransaction(Transaction transaction)
    {
        if (!transaction.IsPaid)
        {
            return Result.Fail<MonthlySummary>(SummaryErrors.TransactionNotPaid);
        }

        if (!GetSummaryPeriod().Contains(transaction.PaymentDate!.Value))
        {
            return Result.Fail<MonthlySummary>(SummaryErrors.TransactionNotInPeriod);
        }

        if (transaction.Amount.Currency != Currency)
        {
            return Result.Fail<MonthlySummary>(SummaryErrors.CurrencyNotMatched);
        }

        if (transaction.Type is TransactionType.Income)
        {
            TotalIncome += transaction.Amount.Amount;
        }
        else
        {
            TotalExpense += transaction.Amount.Amount;
        }

        return this;
    }

    public Result<MonthlySummary> RevertTransaction(Transaction transaction)
    {
        if (transaction.IsPaid)
        {
            return Result.Fail<MonthlySummary>(SummaryErrors.TransactionAlreadyIsPaid);
        }

        if (!GetSummaryPeriod().Contains(transaction.PaymentDate!.Value))
        {
            return Result.Fail<MonthlySummary>(SummaryErrors.TransactionNotInPeriod);
        }

        if (transaction.Amount.Currency != Currency)
        {
            return Result.Fail<MonthlySummary>(SummaryErrors.CurrencyNotMatched);
        }

        if (transaction.Type is TransactionType.Income)
        {
            TotalIncome -= transaction.Amount.Amount;
        }
        else
        {
            TotalExpense -= transaction.Amount.Amount;
        }

        return this;
    }

    public Result<MonthlySummary> UpdateSavings(Money amount, DateOnly assignedOn, bool isIncrement)
    {
        if (!GetSummaryPeriod().Contains(assignedOn))
        {
            return Result.Fail<MonthlySummary>(SummaryErrors.TransactionNotInPeriod);
        }

        if (isIncrement)
        {
            Savings += amount.Amount;
        }
        else
        {
            Savings -= amount.Amount;
        }

        return this;
    }

    private DateRange GetSummaryPeriod()
    {
        DateOnly firstDayOfMonth = new(Year, Month, 1);
        DateOnly lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        return DateRange.Create(firstDayOfMonth, lastDayOfMonth);
    }
}