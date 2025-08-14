using Aurora.Coinly.Domain.Budgets;

namespace Aurora.Coinly.Infrastructure.Budgets;

internal sealed class BudgetRepository(
    ApplicationDbContext dbContext) : BaseRepository<Budget>(dbContext), IBudgetRepository
{
    public IUnitOfWork UnitOfWork => dbContext;

    public async Task AddTransactionAsync(BudgetTransaction budgetTransaction)
    {
        await dbContext.BudgetTransactions.AddAsync(budgetTransaction);
    }

    public async Task<Budget?> GetByIdAsync(Guid id, Guid userId) => await dbContext
        .Budgets
        .Include(x => x.Category)
        .Include(x => x.Periods)
        .ThenInclude(x => x.Transactions)
        .AsSplitQuery()
        .Where(x => x.Id == id && x.UserId == userId)
        .FirstOrDefaultAsync();

    public async Task<Budget?> GetByCategoryIdAsync(Guid userId, Guid categoryId, int year) => await dbContext
        .Budgets
        .Include(x => x.Category)
        .Include(x => x.Periods)
        .ThenInclude(x => x.Transactions)
        .AsSplitQuery()
        .Where(x => x.UserId == userId && x.CategoryId == categoryId && x.Year == year)
        .FirstOrDefaultAsync();

    public async Task<IEnumerable<Budget>> GetListAsync(Guid userId, int year)
    {
        var query = dbContext
            .Budgets
            .Include(x => x.Category)
            .Include(x => x.Periods)
            .AsSplitQuery()
            .Where(x => x.UserId == userId && x.Year == year)
            .AsNoTracking()
            .AsQueryable();

        return await query.OrderBy(x => x.Category.Name).ToListAsync();
    }
}