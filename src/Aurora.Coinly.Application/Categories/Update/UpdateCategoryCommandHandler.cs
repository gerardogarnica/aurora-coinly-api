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

        Result<Color> color = Color.FromHex(request.Color);
        if (!color.IsSuccessful)
        {
            return Result.Fail(color.Error);
        }

        // Update category
        var result = category.Update(
            request.Name,
            request.MaxDaysToReverse,
            color.Value,
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