namespace Aurora.Coinly.Domain.UnitTests.Budgets;

internal static class BudgetData
{
    public static readonly Money AmountLimit = new(10.0m, Currency.Usd);
    public static readonly DateRange Period = DateRange.Create(DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));
    public const string? Notes = "Notes of the budget";
    public const BudgetStatus Status = BudgetStatus.Draft;

    public static Budget GetBudget(Category category) => Budget.Create(
        category,
        AmountLimit,
        Period,
        Notes,
        DateTime.UtcNow);
}