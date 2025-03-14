namespace Aurora.Coinly.Application.Budgets.GetById;

public sealed record GetBudgetByIdQuery(Guid Id) : IQuery<BudgetModel>;