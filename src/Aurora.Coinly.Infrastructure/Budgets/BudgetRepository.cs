using Aurora.Coinly.Domain.Budgets;

namespace Aurora.Coinly.Infrastructure.Budgets;

internal sealed class BudgetRepository(
    ApplicationDbContext dbContext) : BaseRepository<Budget>(dbContext), IBudgetRepository
{
    public IUnitOfWork UnitOfWork => dbContext;

    public async Task<Budget?> GetByIdAsync(Guid id) => await dbContext
        .Budgets
        .Include(x => x.Category)
        .Include(x => x.Transactions)
        .AsSplitQuery()
        .Where(x => x.Id == id)
        .FirstOrDefaultAsync();

    public async Task<Budget?> GetByCategoryIdAsync(Guid categoryId, DateOnly transactionDate) => await dbContext
        .Budgets
        .Include(x => x.Category)
        .Include(x => x.Transactions)
        .AsSplitQuery()
        .Where(x => x.CategoryId == categoryId && x.Period.Start >= transactionDate && x.Period.End <= transactionDate)
        .FirstOrDefaultAsync();

    public async Task<IEnumerable<Budget>> GetListAsync(int year, Guid? categoryId)
    {
        var query = dbContext
            .Budgets
            .Include(x => x.Category)
            .AsSplitQuery()
            .Where(x => x.Period.Start.Year == year)
            .AsNoTracking()
            .AsQueryable();

        if (categoryId is not null)
        {
            query = query.Where(x => x.CategoryId == categoryId.Value);
        }

        return await query.OrderBy(x => x.Period.Start).ToListAsync();
    }
}