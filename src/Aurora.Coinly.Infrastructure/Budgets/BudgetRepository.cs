using Aurora.Coinly.Domain.Budgets;

namespace Aurora.Coinly.Infrastructure.Budgets;

internal sealed class BudgetRepository(
    ApplicationDbContext dbContext) : BaseRepository<Budget>(dbContext), IBudgetRepository
{
    public IUnitOfWork UnitOfWork => dbContext;

    public async Task<Budget?> GetByIdAsync(Guid id) => await dbContext
        .Budgets
        .Include(x => x.Category)
        .Include(x => x.Periods)
        .ThenInclude(x => x.Transactions)
        .AsSplitQuery()
        .Where(x => x.Id == id)
        .FirstOrDefaultAsync();

    public async Task<Budget?> GetByCategoryIdAsync(Guid categoryId, int year) => await dbContext
        .Budgets
        .Include(x => x.Category)
        .Include(x => x.Periods)
        .ThenInclude(x => x.Transactions)
        .AsSplitQuery()
        .Where(x => x.CategoryId == categoryId && x.Year == year)
        .FirstOrDefaultAsync();

    public async Task<IEnumerable<Budget>> GetListAsync(int year)
    {
        var query = dbContext
            .Budgets
            .Include(x => x.Category)
            .Include(x => x.Periods)
            .AsSplitQuery()
            .Where(x => x.Year == year)
            .AsNoTracking()
            .AsQueryable();

        return await query.OrderBy(x => x.Category.Name).ToListAsync();
    }
}