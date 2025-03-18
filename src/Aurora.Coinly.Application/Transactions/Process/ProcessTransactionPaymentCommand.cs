namespace Aurora.Coinly.Application.Transactions.Process;

public sealed record ProcessTransactionPaymentCommand(
    Guid Id,
    Guid WalletId,
    DateOnly PaymentDate) : ICommand;