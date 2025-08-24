namespace Aurora.Coinly.Application.Users.Update;

internal sealed class UpdateUserCommandHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<UpdateUserCommand>
{
    public async Task<Result> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        // Get user
        User? user = await dbContext
            .Users
            .SingleOrDefaultAsync(x => x.Id == userContext.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Fail(UserErrors.NotFound);
        }

        // Update user
        user.Update(
            request.FirstName,
            request.LastName,
            dateTimeService.UtcNow);

        dbContext.Users.Update(user);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}