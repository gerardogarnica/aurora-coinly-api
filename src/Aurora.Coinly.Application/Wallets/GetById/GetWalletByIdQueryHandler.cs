using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Wallets.GetById;

internal sealed class GetWalletByIdQueryHandler(
    IWalletRepository walletRepository) : IQueryHandler<GetWalletByIdQuery, WalletModel>
{
    public async Task<Result<WalletModel>> Handle(
        GetWalletByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Get wallet
        var wallet = await walletRepository.GetByIdAsync(request.Id);
        if (wallet is null)
        {
            return Result.Fail<WalletModel>(WalletErrors.NotFound);
        }

        // Return wallet model
        return wallet.ToModel();
    }
}