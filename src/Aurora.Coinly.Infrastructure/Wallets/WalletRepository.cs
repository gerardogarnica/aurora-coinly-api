using Aurora.Coinly.Domain.Shared;
using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Infrastructure.Wallets;

internal sealed class WalletRepository(
    ApplicationDbContext dbContext) : BaseRepository<Wallet>(dbContext), IWalletRepository
{
    public IUnitOfWork UnitOfWork => dbContext;

    public async Task<Wallet?> GetByIdAsync(Guid id) => await dbContext
        .Wallets
        .Where(x => x.Id == id)
        .FirstOrDefaultAsync();

    public async Task<Wallet?> GetByIdAsync(Guid id, DateRange historyRange) => await dbContext
        .Wallets
        .Include(x => x.Operations)
        .Where(x => x.Id == id)
        .FirstOrDefaultAsync();

    public async Task<IEnumerable<Wallet>> GetListAsync(bool showDeleted)
    {
        var query = dbContext
            .Wallets
            .AsNoTracking()
            .AsQueryable();

        if (showDeleted)
        {
            query = query.Where(x => x.IsDeleted);
        }

        return await query.OrderBy(x => x.Name).ToListAsync();
    }
}