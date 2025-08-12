using Aurora.Coinly.Domain.Categories;

namespace Aurora.Coinly.Application.Categories.GetList;

internal sealed class GetCategoryListQueryHandler(
    ICategoryRepository categoryRepository,
    IUserContext userContext) : IQueryHandler<GetCategoryListQuery, IReadOnlyCollection<CategoryModel>>
{
    public async Task<Result<IReadOnlyCollection<CategoryModel>>> Handle(
        GetCategoryListQuery request,
        CancellationToken cancellationToken)
    {
        // Get categories
        var categories = await categoryRepository.GetListAsync(userContext.UserId, request.ShowDeleted);

        // Return category models
        return categories.Select(x => x.ToModel()).ToList();
    }
}