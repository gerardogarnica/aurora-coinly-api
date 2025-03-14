namespace Aurora.Coinly.Application.Wallets.AssignToSavings;

public sealed record AssignToSavingsCommand(
    Guid WalletId,
    decimal Amount,
    DateOnly AssignedOn) : ICommand;