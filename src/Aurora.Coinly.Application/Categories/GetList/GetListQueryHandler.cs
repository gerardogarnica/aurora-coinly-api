using Aurora.Coinly.Domain.Categories;

namespace Aurora.Coinly.Application.Categories.GetList;

internal sealed class GetListQueryHandler(
    ICategoryRepository categoryRepository) : IQueryHandler<GetListQuery, IReadOnlyCollection<CategoryModel>>
{
    public async Task<Result<IReadOnlyCollection<CategoryModel>>> Handle(
        GetListQuery request,
        CancellationToken cancellationToken)
    {
        // Get categories
        var categories = await categoryRepository.GetListAsync(request.ShowDeleted);

        // Return category models
        return categories.Select(x => x.ToModel()).ToList();
    }
}