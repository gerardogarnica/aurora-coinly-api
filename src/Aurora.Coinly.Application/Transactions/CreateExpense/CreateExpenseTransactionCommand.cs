namespace Aurora.Coinly.Application.Transactions.CreateExpense;

public sealed record CreateExpenseTransactionCommand(
    Guid CategoryId,
    Guid PaymentMethodId,
    string Description,
    DateOnly TransactionDate,
    DateOnly MaxPaymentDate,
    string CurrencyCode,
    decimal Amount,
    string? Notes,
    Guid? WalletId) : ICommand<Guid>;