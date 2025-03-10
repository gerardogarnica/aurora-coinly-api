namespace Aurora.Coinly.Application.Wallets.GetById;

public sealed record GetWalletByIdQuery(Guid Id) : IQuery<WalletModel>;