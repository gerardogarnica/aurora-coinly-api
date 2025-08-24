namespace Aurora.Coinly.Application.Categories.GetById;

internal sealed class GetCategoryByIdQueryHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext) : IQueryHandler<GetCategoryByIdQuery, CategoryModel>
{
    public async Task<Result<CategoryModel>> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Get category
        Category? category = await dbContext
            .Categories
            .SingleOrDefaultAsync(x => x.Id == request.Id && x.UserId == userContext.UserId, cancellationToken);

        if (category is null)
        {
            return Result.Fail<CategoryModel>(CategoryErrors.NotFound);
        }

        // Return category model
        return category.ToModel();
    }
}