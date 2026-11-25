namespace Aurora.Coinly.Application.Authentication.ChangePassword;

internal sealed class ChangeUserPasswordCommandHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IPasswordHasher passwordHasher,
    IDateTimeService dateTimeService) : ICommandHandler<ChangeUserPasswordCommand>
{
    public async Task<Result> Handle(
        ChangeUserPasswordCommand request,
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

        // Validate password
        Result<Password> currentPassword = Password.Create(request.CurrentPassword);
        if (!currentPassword.IsSuccessful || !user.VerifyPassword(currentPassword.Value, passwordHasher))
        {
            return Result.Fail(UserErrors.InvalidCurrentPassword);
        }

        // Encrypt new password
        Result<Password> newPassword = Password.Create(request.NewPassword.Trim());
        if (!newPassword.IsSuccessful)
        {
            return Result.Fail(newPassword.Error);
        }

        string hashedNewPassword = passwordHasher.HashPassword(newPassword.Value.Value);

        // Update user password
        user.UpdatePassword(hashedNewPassword, dateTimeService.UtcNow);

        dbContext.Users.Update(user);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}