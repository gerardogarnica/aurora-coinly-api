namespace Aurora.Coinly.Application.Budgets.UpdateLimit;

public sealed record UpdateBudgetLimitCommand(
    Guid Id,
    Guid PeriodId,
    decimal AmountLimit) : ICommand;