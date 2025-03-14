using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Wallets.GetList;

internal sealed class GetListQueryHandler(
    IWalletRepository walletRepository) : IQueryHandler<GetListQuery, IReadOnlyCollection<WalletModel>>
{
    public async Task<Result<IReadOnlyCollection<WalletModel>>> Handle(
        GetListQuery request,
        CancellationToken cancellationToken)
    {
        // Get wallets
        var wallets = await walletRepository.GetListAsync(request.ShowDeleted);

        // Return wallet models
        return wallets.Select(x => x.ToModel()).ToList();
    }
}