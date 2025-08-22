using Aurora.Coinly.Domain.Users;

namespace Aurora.Coinly.Application.Authentication.ChangePassword;

internal sealed class ChangeUserPasswordCommandHandler(
    IUserRepository userRepository,
    IUserContext userContext,
    IPasswordHasher passwordHasher,
    IDateTimeService dateTimeService) : ICommandHandler<ChangeUserPasswordCommand>
{
    public async Task<Result> Handle(
        ChangeUserPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userContext.UserId);
        if (user is null)
        {
            return Result.Fail(UserErrors.NotFound);
        }

        // Validate password
        var currentPassword = Password.Create(request.CurrentPassword);
        if (!user.VerifyPassword(currentPassword, passwordHasher))
        {
            return Result.Fail(UserErrors.InvalidCurrentPassword);
        }

        // Encrypt new password
        Password newPassword = Password.Create(request.NewPassword.Trim());
        string hashedNewPassword = passwordHasher.HashPassword(newPassword.Value);

        // Update user password
        user.UpdatePassword(hashedNewPassword, dateTimeService.UtcNow);

        userRepository.Update(user);

        return Result.Ok();
    }
}