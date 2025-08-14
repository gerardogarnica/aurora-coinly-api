namespace Aurora.Coinly.Domain.Budgets;

public interface IBudgetRepository : IRepository<Budget>
{
    Task AddTransactionAsync(BudgetTransaction budgetTransaction);
    Task<Budget?> GetByIdAsync(Guid id, Guid userId);
    Task<Budget?> GetByCategoryIdAsync(Guid userId, Guid categoryId, int year);
    Task<IEnumerable<Budget>> GetListAsync(Guid userId, int year);
}