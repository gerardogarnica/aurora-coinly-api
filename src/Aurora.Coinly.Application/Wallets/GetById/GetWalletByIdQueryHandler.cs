namespace Aurora.Coinly.Application.Wallets.GetById;

internal sealed class GetWalletByIdQueryHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext) : IQueryHandler<GetWalletByIdQuery, WalletModel>
{
    public async Task<Result<WalletModel>> Handle(
        GetWalletByIdQuery request,
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

        // Return wallet model
        return wallet.ToModel();
    }
}