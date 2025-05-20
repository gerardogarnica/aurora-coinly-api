namespace Aurora.Coinly.Application.Wallets.Update;

public sealed record UpdateWalletCommand(
    Guid Id,
    string Name,
    bool AllowNegative,
    string Color,
    string? Notes) : ICommand;