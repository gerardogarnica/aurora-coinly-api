using Aurora.Coinly.Domain.Categories;

namespace Aurora.Coinly.Application.Categories.Update;

internal sealed class UpdateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        // Get category
        var category = await categoryRepository.GetByIdAsync(request.Id, userContext.UserId);
        if (category is null)
        {
            return Result.Fail(CategoryErrors.NotFound);
        }

        // Update category
        var result = category.Update(
            request.Name,
            request.MaxDaysToReverse,
            Color.FromHex(request.Color),
            request.Group,
            request.Notes,
            dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        categoryRepository.Update(category);

        return Result.Ok();
    }
}