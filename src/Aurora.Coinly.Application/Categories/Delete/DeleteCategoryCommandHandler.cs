using Aurora.Coinly.Domain.Categories;

namespace Aurora.Coinly.Application.Categories.Delete;

internal sealed class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IDateTimeService dateTimeService) : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> Handle(
        DeleteCategoryCommand request,
        CancellationToken cancellationToken)
    {
        // Get category
        var category = await categoryRepository.GetByIdAsync(request.Id, request.UserId);
        if (category is null)
        {
            return Result.Fail(CategoryErrors.NotFound);
        }

        // Delete category
        var result = category.Delete(dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        categoryRepository.Update(category);

        return Result.Ok();
    }
}