namespace Aurora.Coinly.Application.Transactions.Process;

public sealed record ProcessTransactionPaymentCommand(
    Guid[] TransactionIds,
    Guid WalletId,
    DateOnly PaymentDate) : ICommand;