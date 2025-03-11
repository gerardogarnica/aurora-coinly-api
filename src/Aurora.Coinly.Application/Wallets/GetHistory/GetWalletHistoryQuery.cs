namespace Aurora.Coinly.Application.Wallets.GetHistory;

public sealed record GetWalletHistoryQuery(
    Guid Id,
    DateOnly DateFrom,
    DateOnly DateTo) : IQuery<WalletModel>;