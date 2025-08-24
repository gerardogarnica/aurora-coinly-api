namespace Aurora.Coinly.Application.Categories.GetList;

internal sealed class GetCategoryListQueryHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext) : IQueryHandler<GetCategoryListQuery, IReadOnlyCollection<CategoryModel>>
{
    public async Task<Result<IReadOnlyCollection<CategoryModel>>> Handle(
        GetCategoryListQuery request,
        CancellationToken cancellationToken)
    {
        // Get categories
        var query = dbContext
            .Categories
            .Where(x => x.UserId == userContext.UserId)
            .AsNoTracking()
            .AsQueryable();

        if (!request.ShowDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        List<Category> categories = await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);

        // Return category models
        return categories.Select(x => x.ToModel()).ToList();
    }
}