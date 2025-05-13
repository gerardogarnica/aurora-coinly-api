namespace Aurora.Coinly.Application.Budgets.GetList;

public sealed record GetBudgetListQuery(int Year) : IQuery<IReadOnlyCollection<BudgetModel>>;