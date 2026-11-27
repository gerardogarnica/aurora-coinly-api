namespace Aurora.Coinly.Application.Authentication.ChangePassword;

internal sealed class ChangePasswordCommandHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IPasswordHasher passwordHasher,
    IDateTimeService dateTimeService) : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(
        ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        DateTime utcNow = dateTimeService.UtcNow;

        // Only tokens that can still be redeemed; revoked and expired ones need no revocation.
        User? user = await dbContext
            .Users
            .Include(u => u.Tokens.Where(t => !t.IsRevoked && t.RefreshTokenExpiresOnUtc > utcNow))
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.Id == userContext.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Fail(UserErrors.NotFound);
        }

        if (!user.IsActive)
        {
            return Result.Fail(UserErrors.Inactive);
        }

        if (!user.VerifyPassword(passwordHasher, command.CurrentPassword))
        {
            return Result.Fail(AuthenticationErrors.InvalidCurrentPassword);
        }

        string passwordHash = passwordHasher.HashPassword(command.NewPassword);

        Result<Password> passwordResult = Password.Create(passwordHash);
        if (!passwordResult.IsSuccessful)
        {
            return Result.Fail(passwordResult.Error);
        }

        Result changePasswordResult = user.ChangePassword(passwordResult.Value, utcNow);
        if (!changePasswordResult.IsSuccessful)
        {
            return Result.Fail(changePasswordResult.Error);
        }

        Result revokeResult = user.RevokeAllActiveTokens(utcNow);
        if (!revokeResult.IsSuccessful)
        {
            return Result.Fail(revokeResult.Error);
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // A refresh rotated one of these tokens, or another request changed the user, between our read
            // and our write (xmin changed). Nothing was saved; the client's retry also revokes the rotated token.
            return Result.Fail(AuthenticationErrors.ConcurrentSessionChange);
        }

        return Result.Ok();
    }
}
