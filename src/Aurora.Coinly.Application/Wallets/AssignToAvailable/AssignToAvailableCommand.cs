namespace Aurora.Coinly.Application.Wallets.AssignToAvailable;

public sealed record AssignToAvailableCommand(
    Guid WalletId,
    decimal Amount,
    DateOnly AssignedOn) : ICommand;