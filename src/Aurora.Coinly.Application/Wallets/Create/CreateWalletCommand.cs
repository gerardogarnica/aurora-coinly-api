using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Wallets.Create;

public sealed record CreateWalletCommand(
    string Name,
    string CurrencyCode,
    decimal Amount,
    WalletType Type,
    string? Notes) : IRequest<Guid>;