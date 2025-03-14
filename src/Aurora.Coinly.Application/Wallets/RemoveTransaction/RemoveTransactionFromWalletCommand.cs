namespace Aurora.Coinly.Application.Wallets.RemoveTransaction;

public sealed record RemoveTransactionFromWalletCommand(
    Guid WalletId,
    Guid TransactionId) : ICommand;