namespace Aurora.Coinly.Application.Authentication.Login;

internal sealed class LoginCommandHandler(
    ICoinlyDbContext dbContext,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider,
    IDateTimeService dateTimeService) : ICommandHandler<LoginCommand, IdentityToken>
{
    public async Task<Result<IdentityToken>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        Result<Email> emailResult = Email.Create(command.Email);
        if (!emailResult.IsSuccessful)
        {
            passwordHasher.VerifyDummy(command.Password);
            return Result.Fail<IdentityToken>(AuthenticationErrors.InvalidCredentials);
        }

        string emailValue = emailResult.Value.Value;

        User? user = await dbContext
            .Users
            .Include(x => x.Roles)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Email.Value == emailValue, cancellationToken);

        if (user is null)
        {
            passwordHasher.VerifyDummy(command.Password);
            return Result.Fail<IdentityToken>(AuthenticationErrors.InvalidCredentials);
        }

        // Validate password
        bool passwordValid = user.VerifyPassword(passwordHasher, command.Password);

        if (!user.IsActive || !passwordValid)
        {
            return Result.Fail<IdentityToken>(AuthenticationErrors.InvalidCredentials);
        }

        List<string> roles = [.. user.Roles.Select(r => r.Name)];

        // Create token
        TokenRequest tokenRequest = new(
            user.Id,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            roles);

        IssuedToken issuedToken = tokenProvider.CreateToken(tokenRequest);

        // Create user token
        Result<UserToken> issueResult = user.IssueToken(
            issuedToken.AccessTokenId,
            issuedToken.RefreshTokenHash,
            issuedToken.Identity.AccessTokenExpiresOn.UtcDateTime,
            issuedToken.Identity.RefreshTokenExpiresOn.UtcDateTime,
            dateTimeService.UtcNow);

        if (!issueResult.IsSuccessful)
        {
            return Result.Fail<IdentityToken>(issueResult.Error);
        }

        dbContext.UserTokens.Add(issueResult.Value);

        await dbContext.SaveChangesAsync(cancellationToken);

        // Return identity token
        return issuedToken.Identity;
    }
}
