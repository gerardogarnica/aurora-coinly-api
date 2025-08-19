using Aurora.Coinly.Domain.Users;

namespace Aurora.Coinly.Application.Users.GetById;

internal sealed class GetUserByIdQueryHandler(
    IUserRepository userRepository) : IQueryHandler<GetUserByIdQuery, UserModel>
{
    public async Task<Result<UserModel>> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Get user
        var user = await userRepository.GetByIdAsync(request.Id);
        if (user is null)
        {
            return Result.Fail<UserModel>(UserErrors.NotFound);
        }

        // Return user model
        return user.ToModel();
    }
}