using Aurora.Coinly.Domain.Categories;

namespace Aurora.Coinly.Application.Categories.Create;

internal sealed class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IDateTimeService dateTimeService) : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        // Create category
        var category = Category.Create(
            request.UserId,
            request.Name,
            request.Group,
            request.Type,
            request.MaxDaysToReverse,
            Color.FromHex(request.Color),
            request.Notes,
            dateTimeService.UtcNow);

        await categoryRepository.AddAsync(category, cancellationToken);

        return category.Id;
    }
}