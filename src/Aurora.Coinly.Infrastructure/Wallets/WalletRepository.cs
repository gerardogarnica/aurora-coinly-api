using Aurora.Coinly.Domain.Shared;
using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Infrastructure.Wallets;

internal sealed class WalletRepository(
    ApplicationDbContext dbContext) : BaseRepository<Wallet>(dbContext), IWalletRepository
{
    public IUnitOfWork UnitOfWork => dbContext;

    public async Task<Wallet?> GetByIdAsync(Guid id, Guid userId)
    {
        var wallet = await dbContext
            .Wallets
            .Include(x => x.Methods)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (wallet is null)
        {
            return null;
        }

        SetOpenAndLastOperationDates(wallet);

        return wallet;
    }

    public async Task<Wallet?> GetByIdAsync(Guid id, Guid userId, DateRange historyRange)
    {
        var wallet = await GetByIdAsync(id, userId);

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

    public async Task<IEnumerable<Wallet>> GetListAsync(Guid userId, bool showDeleted)
    {
        var query = dbContext
            .Wallets
            .Include(x => x.Methods)
            .Where(x => x.UserId == userId)
            .AsNoTracking()
            .AsQueryable();

        if (!showDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        var wallets = await query.OrderBy(x => x.Name).ToListAsync();
        foreach (var wallet in wallets)
        {
            SetOpenAndLastOperationDates(wallet);
        }

        return wallets;
    }

    private void SetOpenAndLastOperationDates(Wallet wallet)
    {
        var openedOnDate = GetOpenedOnDate(wallet);
        var lastOperationOn = GetLastOperationDate(wallet);
        wallet.SetOpenAndLastOperationDates(openedOnDate.OperationOn, lastOperationOn.OperationOn);
    }

    private WalletOperationDate GetOpenedOnDate(Wallet wallet)
    {
        string sql = $"""
            SELECT "date"
            FROM {ApplicationDbContext.DEFAULT_SCHEMA}.wallet_history
            WHERE wallet_id = '{wallet.Id}'
            AND type = 'Created'
            LIMIT 1
            """;

        IEnumerable<WalletOperationDate> openedOn = dbContext
            .Database
            .SqlQueryRaw<WalletOperationDate>(sql);

        return openedOn.FirstOrDefault();
    }

    private WalletOperationDate GetLastOperationDate(Wallet wallet)
    {
        string sql = $"""
            SELECT "date"
            FROM {ApplicationDbContext.DEFAULT_SCHEMA}.wallet_history
            WHERE wallet_id = '{wallet.Id}'
            ORDER BY date desc
            LIMIT 1
            """;

        IEnumerable<WalletOperationDate> lastOperationOn = dbContext
            .Database
            .SqlQueryRaw<WalletOperationDate>(sql);

        return lastOperationOn.FirstOrDefault();
    }

    internal sealed record WalletOperationDate(DateOnly OperationOn);
}