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
            request.Name,
            request.Type,
            request.Notes,
            dateTimeService.UtcNow);

        await categoryRepository.AddAsync(category, cancellationToken);

        return category.Id;
    }
}