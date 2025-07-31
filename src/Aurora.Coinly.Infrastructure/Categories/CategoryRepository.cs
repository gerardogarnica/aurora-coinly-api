using Aurora.Coinly.Domain.Categories;

namespace Aurora.Coinly.Infrastructure.Categories;

internal sealed class CategoryRepository(
    ApplicationDbContext dbContext) : BaseRepository<Category>(dbContext), ICategoryRepository
{
    public IUnitOfWork UnitOfWork => dbContext;

    public async Task<Category?> GetByIdAsync(Guid id, Guid userId) => await dbContext
        .Categories
        .Where(x => x.Id == id && x.UserId == userId)
        .FirstOrDefaultAsync();

    public async Task<IEnumerable<Category>> GetListAsync(Guid userId, bool showDeleted)
    {
        var query = dbContext
            .Categories
            .Where(x => x.UserId == userId)
            .AsNoTracking()
            .AsQueryable();

        if (!showDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        return await query.OrderBy(x => x.Name).ToListAsync();
    }
}