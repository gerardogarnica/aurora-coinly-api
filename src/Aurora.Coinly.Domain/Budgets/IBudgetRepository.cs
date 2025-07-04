namespace Aurora.Coinly.Domain.Budgets;

public interface IBudgetRepository : IRepository<Budget>
{
    Task AddTransactionAsync(BudgetTransaction budgetTransaction);
    Task<Budget?> GetByIdAsync(Guid id);
    Task<Budget?> GetByCategoryIdAsync(Guid categoryId, int year);
    Task<IEnumerable<Budget>> GetListAsync(int year);
}