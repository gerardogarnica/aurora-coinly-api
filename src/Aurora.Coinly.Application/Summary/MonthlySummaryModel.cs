using Aurora.Coinly.Domain.Summary;

namespace Aurora.Coinly.Application.Summary;

public sealed record MonthlySummaryModel(
    int Year,
    int Month,
    string CurrencyCode,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance,
    decimal Savings);

internal static class MonthlySummaryModelExtensions
{
    public static MonthlySummaryModel ToModel(this MonthlySummary summary)
    {
        return new MonthlySummaryModel(
            summary.Year,
            summary.Month,
            summary.Currency.Code,
            summary.TotalIncome,
            summary.TotalExpense,
            summary.Balance,
            summary.Savings);
    }
}