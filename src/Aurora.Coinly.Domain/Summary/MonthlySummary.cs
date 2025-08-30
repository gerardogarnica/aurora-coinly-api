using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Domain.Summary;

public sealed class MonthlySummary : BaseEntity
{
    public Guid UserId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public string CurrencyCode { get; private set; }
    public decimal TotalIncome { get; private set; }
    public decimal TotalExpense { get; private set; }
    public decimal Balance => TotalIncome - TotalExpense;
    public decimal Savings { get; private set; }

    private MonthlySummary() : base(Guid.NewGuid()) { }

    public static IList<MonthlySummary> Create(
        Guid userId,
        int year,
        string currencyCode,
        decimal savingsAmount)
    {
        List<MonthlySummary> summaries = [];

        for (int month = 1; month <= 12; month++)
        {
            MonthlySummary summary = new()
            {
                UserId = userId,
                Year = year,
                Month = month,
                CurrencyCode = currencyCode,
                TotalIncome = decimal.Zero,
                TotalExpense = decimal.Zero,
                Savings = savingsAmount
            };

            summaries.Add(summary);
        }

        return summaries;
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

        if (transaction.Amount.Currency.Code != CurrencyCode)
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

        if (transaction.Amount.Currency.Code != CurrencyCode)
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

    public void UpdateSavings(Money amount, bool isIncrement)
    {
        Savings = isIncrement
            ? Savings + amount.Amount
            : Savings - amount.Amount;
    }

    private DateRange GetSummaryPeriod()
    {
        DateOnly firstDayOfMonth = new(Year, Month, 1);
        DateOnly lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        return DateRange.Create(firstDayOfMonth, lastDayOfMonth);
    }
}