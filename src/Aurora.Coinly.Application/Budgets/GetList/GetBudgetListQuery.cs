namespace Aurora.Coinly.Application.Budgets.GetList;

public sealed record GetBudgetListQuery(int Year, Guid? CategoryId) : IQuery<IReadOnlyCollection<BudgetModel>>;