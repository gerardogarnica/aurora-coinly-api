namespace Aurora.Coinly.Application.Budgets;

public sealed record BudgetPeriodModel(
    Guid PeriodId,
    DateOnly PeriodBegins,
    DateOnly PeriodEnds,
    string CurrencyCode,
    decimal AmountLimit,
    decimal SpentAmount,
    bool IsExceeded,
    List<BudgetTransactionModel> Transactions);

internal static class BudgetPeriodModelExtensions
{
    internal static BudgetPeriodModel ToModel(this BudgetPeriod period) => new(
        period.Id,
        period.Period.Start,
        period.Period.End,
        period.Limit.Currency.Code,
        period.Limit.Amount,
        period.GetSpentAmount().Amount,
        period.IsExceeded(),
        [.. period.Transactions.Select(x => x.ToModel()).OrderBy(x => x.Date)]);
}