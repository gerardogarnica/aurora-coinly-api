using Aurora.Coinly.Domain.Categories;

namespace Aurora.Coinly.Infrastructure.Categories;

internal sealed class CategoryRepository(
    ApplicationDbContext dbContext) : BaseRepository<Category>(dbContext), ICategoryRepository
{
    public IUnitOfWork UnitOfWork => dbContext;

    public async Task<Category?> GetByIdAsync(Guid id) => await dbContext
        .Categories
        .Where(x => x.Id == id)
        .FirstOrDefaultAsync();

    public async Task<IEnumerable<Category>> GetListAsync(bool showDeleted)
    {
        var query = dbContext
            .Categories
            .AsNoTracking()
            .AsQueryable();

        if (showDeleted)
        {
            query = query.Where(x => x.IsDeleted);
        }

        return await query.OrderBy(x => x.Name).ToListAsync();
    }
}
