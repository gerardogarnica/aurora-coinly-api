namespace Aurora.Coinly.Application.Wallets.AddTransaction;

public sealed record AddTransactionToWalletCommand(
    Guid WalletId,
    Guid TransactionId) : ICommand;