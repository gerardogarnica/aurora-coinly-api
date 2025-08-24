namespace Aurora.Coinly.Application.Categories.Delete;

internal sealed class DeleteCategoryCommandHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> Handle(
        DeleteCategoryCommand request,
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

        // Delete category
        var result = category.Delete(dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        dbContext.Categories.Update(category);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}