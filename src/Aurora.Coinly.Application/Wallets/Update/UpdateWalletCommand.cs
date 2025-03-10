namespace Aurora.Coinly.Application.Wallets.Update;

public sealed record UpdateWalletCommand(
    Guid WalletId,
    string Name,
    string? Notes) : ICommand;