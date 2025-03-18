namespace Aurora.Coinly.Application.Wallets.GetList;

public sealed record GetWalletListQuery(bool ShowDeleted) : IQuery<IReadOnlyCollection<WalletModel>>;