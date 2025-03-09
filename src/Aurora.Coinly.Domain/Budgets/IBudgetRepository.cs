namespace Aurora.Coinly.Domain.Budgets;

public interface IBudgetRepository : IRepository<Budget>
{
    Task<Budget?> GetByIdAsync(Guid id);
    Task<IEnumerable<Budget>> GetListAsync(
        int year,
        Guid? categoryId);
}