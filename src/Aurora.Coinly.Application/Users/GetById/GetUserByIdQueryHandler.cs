namespace Aurora.Coinly.Application.Users.GetById;

internal sealed class GetUserByIdQueryHandler(
    ICoinlyDbContext dbContext) : IQueryHandler<GetUserByIdQuery, UserModel>
{
    public async Task<Result<UserModel>> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Get user
        User? user = await dbContext
            .Users
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (user is null)
        {
            return Result.Fail<UserModel>(UserErrors.NotFound);
        }

        // Return user model
        return user.ToModel();
    }
}