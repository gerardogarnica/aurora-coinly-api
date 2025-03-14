namespace Aurora.Coinly.Application.Budgets.AddTransaction;

public sealed record AddTransactionToBudgetCommand(
    Guid BudgetId,
    Guid TransactionId) : ICommand;