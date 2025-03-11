namespace Aurora.Coinly.Application.Wallets.GetList;

public sealed record GetListQuery(bool ShowDeleted) : IQuery<IReadOnlyCollection<WalletModel>>;