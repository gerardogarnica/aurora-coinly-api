namespace Aurora.Coinly.Application.Budgets.Update;

public sealed record UpdateBudgetCommand(
    Guid Id,
    decimal AmountLimit,
    DateOnly PeriodBegins,
    DateOnly PeriodEnds,
    string? Notes) : ICommand;