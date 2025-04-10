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
        .AsSplitQuery()
        .Where(x => x.Id == id)
        .FirstOrDefaultAsync();

    public async Task<IEnumerable<Transaction>> GetListAsync(
        DateRange dateRange,
        TransactionStatus? status,
        Guid? categoryId,
        Guid? paymentMethodId)
    {
        var query = dbContext
            .Transactions
            .Include(x => x.Category)
            .Include(x => x.PaymentMethod)
            .AsSplitQuery()
            .Where(x => x.TransactionDate >= dateRange.Start && x.TransactionDate <= dateRange.End)
            .AsNoTracking()
            .AsQueryable();

        if (status is not null)
        {
            query = query.Where(x => x.Status == status);
        }

        if (categoryId is not null)
        {
            query = query.Where(x => x.CategoryId == categoryId.Value);
        }

        if (paymentMethodId is not null)
        {
            query = query.Where(x => x.PaymentMethodId == paymentMethodId.Value);
        }

        return await query.OrderBy(x => x.TransactionDate).ToListAsync();
    }
}