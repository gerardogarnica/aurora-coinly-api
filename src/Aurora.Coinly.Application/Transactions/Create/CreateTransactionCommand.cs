namespace Aurora.Coinly.Application.Transactions.Create;

public sealed record CreateTransactionCommand(
    string Description,
    Guid CategoryId,
    Guid PaymentMethodId,
    DateOnly TransactionDate,
    DateOnly MaxPaymentDate,
    string CurrencyCode,
    decimal Amount,
    string? Notes,
    int Installment,
    Guid? WalletId) : ICommand<Guid>;