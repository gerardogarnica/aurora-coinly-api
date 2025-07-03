namespace Aurora.Coinly.Application.Transactions.CreateExpense;

public sealed record CreateExpenseTransactionCommand(
    Guid CategoryId,
    Guid PaymentMethodId,
    string Description,
    DateOnly TransactionDate,
    string CurrencyCode,
    decimal Amount,
    string? Notes,
    bool MakePayment,
    Guid? WalletId) : ICommand<Guid>;