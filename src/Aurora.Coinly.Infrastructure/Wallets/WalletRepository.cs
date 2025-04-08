using Aurora.Coinly.Domain.Shared;
using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Infrastructure.Wallets;

internal sealed class WalletRepository(
    ApplicationDbContext dbContext) : BaseRepository<Wallet>(dbContext), IWalletRepository
{
    public IUnitOfWork UnitOfWork => dbContext;

    public async Task<Wallet?> GetByIdAsync(Guid id) => await dbContext
        .Wallets
        .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Wallet?> GetByIdAsync(Guid id, DateRange historyRange)
    {
        var wallet = await GetByIdAsync(id);

        if (wallet is not null)
        {
            var operations = await dbContext
                .WalletHistories
                .Where(x => x.WalletId == wallet.Id && x.Date >= historyRange.Start && x.Date <= historyRange.End)
                .OrderBy(x => x.Date)
                .ThenBy(x => x.CreatedOnUtc)
                .ToListAsync();

            wallet.SetOperations(operations);
        }

        return wallet;
    }

    public async Task<IEnumerable<Wallet>> GetListAsync(bool showDeleted)
    {
        var query = dbContext
            .Wallets
            .AsNoTracking()
            .AsQueryable();

        if (!showDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        return await query.OrderBy(x => x.Name).ToListAsync();
    }
}