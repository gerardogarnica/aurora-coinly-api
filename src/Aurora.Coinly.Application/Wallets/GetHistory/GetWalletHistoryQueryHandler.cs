using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Wallets.GetHistory;

internal sealed class GetWalletHistoryQueryHandler(
    IWalletRepository walletRepository,
    IUserContext userContext) : IQueryHandler<GetWalletHistoryQuery, WalletModel>
{
    public async Task<Result<WalletModel>> Handle(
        GetWalletHistoryQuery request,
        CancellationToken cancellationToken)
    {
        // Get wallet with history
        var dateRange = DateRange.Create(request.DateFrom, request.DateTo);
        var wallet = await walletRepository.GetByIdAsync(request.Id, userContext.UserId, dateRange);
        if (wallet is null)
        {
            return Result.Fail<WalletModel>(WalletErrors.NotFound);
        }

        // Return wallet model
        return wallet.ToModel();
    }
}