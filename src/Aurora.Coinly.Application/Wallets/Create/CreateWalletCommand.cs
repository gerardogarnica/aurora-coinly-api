namespace Aurora.Coinly.Application.Wallets.Create;

public sealed record CreateWalletCommand(
    string Name,
    string CurrencyCode,
    decimal Amount,
    WalletType Type,
    bool AllowNegative,
    string Color,
    string? Notes,
    DateOnly OpenedOn) : ICommand<Guid>;