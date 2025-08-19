using Aurora.Coinly.Domain.Users;

namespace Aurora.Coinly.Application.Users.Update;

internal sealed class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<UpdateUserCommand>
{
    public async Task<Result> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userContext.UserId);
        if (user is null)
        {
            return Result.Fail(UserErrors.NotFound);
        }

        // Update user
        user.Update(
            request.FirstName,
            request.LastName,
            dateTimeService.UtcNow);

        userRepository.Update(user);

        return Result.Ok();
    }
}