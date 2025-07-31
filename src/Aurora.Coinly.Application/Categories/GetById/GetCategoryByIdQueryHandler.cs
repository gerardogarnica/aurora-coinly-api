using Aurora.Coinly.Domain.Categories;

namespace Aurora.Coinly.Application.Categories.GetById;

internal sealed class GetCategoryByIdQueryHandler(
    ICategoryRepository categoryRepository) : IQueryHandler<GetCategoryByIdQuery, CategoryModel>
{
    public async Task<Result<CategoryModel>> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Get category
        var category = await categoryRepository.GetByIdAsync(request.Id, request.UserId);
        if (category is null)
        {
            return Result.Fail<CategoryModel>(CategoryErrors.NotFound);
        }

        // Return category model
        return category.ToModel();
    }
}