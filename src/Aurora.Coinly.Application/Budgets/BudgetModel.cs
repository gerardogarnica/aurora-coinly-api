using Aurora.Coinly.Application.Categories;
using Aurora.Coinly.Domain.Budgets;

namespace Aurora.Coinly.Application.Budgets;

public sealed record BudgetModel(
    Guid BudgetId,
    Guid CategoryId,
    CategoryModel Category,
    int BudgetYear,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    BudgetFrequency Frequency,
    List<BudgetPeriodModel> Periods);

internal static class BudgetModelExtensions
{
    internal static BudgetModel ToModel(this Budget budget) => new(
        budget.Id,
        budget.Category.Id,
        budget.Category.ToModel(),
        budget.Year,
        budget.Frequency,
        [.. budget.Periods.Select(x => x.ToModel()).OrderBy(x => x.PeriodBegins)]);
}