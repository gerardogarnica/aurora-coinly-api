namespace Aurora.Coinly.Application.Budgets.Create;

public sealed record CreateBudgetCommand(
    Guid CategoryId,
    string CurrencyCode,
    decimal AmountLimit,
    DateOnly PeriodBegins,
    DateOnly PeriodEnds,
    string? Notes) : ICommand<Guid>;