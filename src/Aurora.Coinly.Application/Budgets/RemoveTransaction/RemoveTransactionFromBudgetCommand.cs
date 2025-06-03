namespace Aurora.Coinly.Application.Budgets.RemoveTransaction;

public sealed record RemoveTransactionFromBudgetCommand(
    Guid TransactionId,
    DateOnly OriginalPaymentDate) : ICommand;