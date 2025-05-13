namespace Aurora.Coinly.Domain.Budgets;

public interface IBudgetRepository : IRepository<Budget>
{
    Task<Budget?> GetByIdAsync(Guid id);
    Task<Budget?> GetByCategoryIdAsync(Guid categoryId, int year);
    Task<IEnumerable<Budget>> GetListAsync(
        int year,
        Guid? categoryId);
}