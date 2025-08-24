namespace Aurora.Coinly.Application.Wallets.GetList;

internal sealed class GetWalletListQueryHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext) : IQueryHandler<GetWalletListQuery, IReadOnlyCollection<WalletModel>>
{
    public async Task<Result<IReadOnlyCollection<WalletModel>>> Handle(
        GetWalletListQuery request,
        CancellationToken cancellationToken)
    {
        // Get wallets
        var query = dbContext
            .Wallets
            .Include(x => x.Methods)
            .Where(x => x.UserId == userContext.UserId)
            .AsNoTracking()
            .AsQueryable();

        if (!request.ShowDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        List<Wallet> wallets = await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);

        // Return wallet models
        return wallets.Select(x => x.ToModel()).ToList();
    }
}