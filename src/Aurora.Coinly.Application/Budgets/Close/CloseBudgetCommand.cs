namespace Aurora.Coinly.Application.Budgets.Close;

public sealed record CloseBudgetCommand(Guid Id) : ICommand;