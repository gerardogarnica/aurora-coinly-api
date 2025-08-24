namespace Aurora.Coinly.Application.Categories.Update;

internal sealed class UpdateCategoryCommandHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        // Get category
        Category? category = await dbContext
            .Categories
            .SingleOrDefaultAsync(x => x.Id == request.Id && x.UserId == userContext.UserId, cancellationToken);

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

        dbContext.Categories.Update(category);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}