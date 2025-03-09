namespace Aurora.Coinly.Domain.Categories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByIdAsync(Guid id);
    Task<IEnumerable<Category>> GetListAsync(bool showDeleted);
}