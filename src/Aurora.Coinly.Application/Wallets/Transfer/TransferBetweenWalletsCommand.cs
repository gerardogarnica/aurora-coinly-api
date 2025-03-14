namespace Aurora.Coinly.Application.Wallets.Transfer;

public sealed record TransferBetweenWalletsCommand(
    Guid SourceWalletId,
    Guid DestinationWalletId,
    decimal Amount,
    DateOnly TransferedOn) : ICommand;