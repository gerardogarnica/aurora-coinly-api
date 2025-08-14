using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Wallets.GetList;

internal sealed class GetWalletListQueryHandler(
    IWalletRepository walletRepository,
    IUserContext userContext) : IQueryHandler<GetWalletListQuery, IReadOnlyCollection<WalletModel>>
{
    public async Task<Result<IReadOnlyCollection<WalletModel>>> Handle(
        GetWalletListQuery request,
        CancellationToken cancellationToken)
    {
        // Get wallets
        var wallets = await walletRepository.GetListAsync(userContext.UserId, request.ShowDeleted);

        // Return wallet models
        return wallets.Select(x => x.ToModel()).ToList();
    }
}