namespace Aurora.Coinly.Application.Wallets.Delete;

public sealed record DeleteWalletCommand(Guid Id) : ICommand;