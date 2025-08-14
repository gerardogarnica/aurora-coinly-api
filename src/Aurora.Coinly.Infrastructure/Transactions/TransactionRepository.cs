using Aurora.Coinly.Domain.Shared;
using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Infrastructure.Transactions;

internal sealed class TransactionRepository(
    ApplicationDbContext dbContext) : BaseRepository<Transaction>(dbContext), ITransactionRepository
{
    public IUnitOfWork UnitOfWork => dbContext;

    public async Task<Transaction?> GetByIdAsync(Guid id) => await dbContext
        .Transactions
        .Include(x => x.Category)
        .Include(x => x.PaymentMethod)
        .Include(x => x.Wallet)
        .AsSplitQuery()
        .Where(x => x.Id == id)
        .FirstOrDefaultAsync();

    public async Task<Transaction?> GetByIdAsync(Guid id, Guid userId) => await dbContext
        .Transactions
        .Include(x => x.Category)
        .Include(x => x.PaymentMethod)
        .Include(x => x.Wallet)
        .AsSplitQuery()
        .Where(x => x.Id == id && x.UserId == userId)
        .FirstOrDefaultAsync();

    public async Task<IEnumerable<Transaction>> GetListAsync(
        Guid userId,
        DateRange dateRange,
        TransactionStatus? status,
        Guid? categoryId,
        Guid? paymentMethodId,
        DisplayDateType displayDateType)
    {
        var query = dbContext
            .Transactions
            .Include(x => x.Category)
            .Include(x => x.PaymentMethod)
            .Include(x => x.Wallet)
            .AsSplitQuery()
            .Where(x => x.UserId == userId)
            .Where(x => displayDateType == DisplayDateType.TransactionDate
                ? x.TransactionDate >= dateRange.Start && x.TransactionDate <= dateRange.End
                : x.PaymentDate! >= dateRange.Start && x.PaymentDate! <= dateRange.End)
            .Where(x => status == null || x.Status == status)
            .Where(x => categoryId == null || x.CategoryId == categoryId.Value)
            .Where(x => paymentMethodId == null || x.PaymentMethodId == paymentMethodId.Value)
            .AsNoTracking()
            .AsQueryable();

        return await query.OrderBy(x => x.TransactionDate).ToListAsync();
    }
}