namespace Aurora.Coinly.Domain.Budgets;

public interface IBudgetRepository : IRepository<Budget>
{
    Task<Budget?> GetByIdAsync(Guid id);
    Task<Budget?> GetByCategoryIdAsync(Guid categoryId, DateOnly transactionDate);
    Task<IEnumerable<Budget>> GetListAsync(
        int year,
        Guid? categoryId);
}