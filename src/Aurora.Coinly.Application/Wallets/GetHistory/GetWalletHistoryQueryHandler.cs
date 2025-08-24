namespace Aurora.Coinly.Application.Wallets.GetHistory;

internal sealed class GetWalletHistoryQueryHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext) : IQueryHandler<GetWalletHistoryQuery, WalletModel>
{
    public async Task<Result<WalletModel>> Handle(
        GetWalletHistoryQuery request,
        CancellationToken cancellationToken)
    {
        // Get wallet
        Wallet? wallet = await dbContext
            .Wallets
            .Include(x => x.Methods)
            .SingleOrDefaultAsync(x => x.Id == request.Id && x.UserId == userContext.UserId, cancellationToken);

        if (wallet is null)
        {
            return Result.Fail<WalletModel>(WalletErrors.NotFound);
        }

        // Get wallet history
        DateRange dateRange = DateRange.Create(request.DateFrom, request.DateTo);

        List<WalletHistory> operations = await dbContext
            .WalletHistories
            .Where(x => x.WalletId == wallet.Id && x.Date >= dateRange.Start && x.Date <= dateRange.End)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.CreatedOnUtc)
            .ToListAsync(cancellationToken);

        wallet.SetOperations(operations);

        // Return wallet model
        return wallet.ToModel();
    }
}