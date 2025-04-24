using Aurora.Coinly.Domain.Budgets;

namespace Aurora.Coinly.Application.Budgets;

public sealed record BudgetModel(
    Guid BudgetId,
    Guid CategoryId,
    string CategoryName,
    string CurrencyCode,
    decimal AmountLimit,
    DateOnly PeriodBegins,
    DateOnly PeriodEnds,
    decimal SpentAmount,
    bool IsExceeded,
    string? Notes,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    BudgetStatus Status,
    List<BudgetTransactionModel> Transactions);

internal static class BudgetModelExtensions
{
    internal static BudgetModel ToModel(this Budget budget) => new(
        budget.Id,
        budget.Category.Id,
        budget.Category.Name,
        budget.AmountLimit.Currency.Code,
        budget.AmountLimit.Amount,
        budget.Period.Start,
        budget.Period.End,
        budget.GetSpentAmount().Amount,
        budget.IsExceeded(),
        budget.Notes,
        budget.Status,
        [.. budget.Transactions.Select(x => x.ToModel()).OrderBy(x => x.Date)]);
}