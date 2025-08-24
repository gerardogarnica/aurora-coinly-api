namespace Aurora.Coinly.Application.Budgets.Create;

public sealed record CreateBudgetCommand(
    Guid CategoryId,
    int Year,
    BudgetFrequency Frequency,
    string CurrencyCode,
    decimal AmountLimit) : ICommand<Guid>;