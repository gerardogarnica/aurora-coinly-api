namespace Aurora.Coinly.Domain.UnitTests.Budgets;

internal static class BudgetData
{
    public static readonly int Year = DateTime.UtcNow.Year;
    public const BudgetFrequency Frequency = BudgetFrequency.Monthly;
    public static readonly Money AmountLimit = new(10.0m, Currency.Usd);

    public static Budget GetBudget(Category category) => Budget.Create(
        category,
        Year,
        Frequency,
        AmountLimit,
        DateTime.UtcNow,
        new BudgetPeriodService());
}