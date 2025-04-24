namespace Aurora.Coinly.Application.Wallets.AddTransaction;

public sealed record AddTransactionToWalletCommand(Guid TransactionId) : ICommand;