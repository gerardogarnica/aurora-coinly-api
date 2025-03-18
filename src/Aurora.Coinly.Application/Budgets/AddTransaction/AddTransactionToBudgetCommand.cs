namespace Aurora.Coinly.Application.Budgets.AddTransaction;

<<<<<<< HEAD
public sealed record AddTransactionToBudgetCommand(
    Guid BudgetId,
    Guid TransactionId) : ICommand;
=======
public sealed record AddTransactionToBudgetCommand(Guid TransactionId) : ICommand;
>>>>>>> development
