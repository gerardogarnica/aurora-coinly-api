namespace Aurora.Coinly.Domain.Categories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<Category>> GetListAsync(Guid userId, bool showDeleted);
}