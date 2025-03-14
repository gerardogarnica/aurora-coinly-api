namespace Aurora.Coinly.Application.Budgets.RemoveTransaction;

public sealed record RemoveTransactionFromBudgetCommand(
    Guid BudgetId,
    Guid TransactionId) : ICommand;