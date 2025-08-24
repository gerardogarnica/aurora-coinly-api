namespace Aurora.Coinly.Application.Authentication.Refresh;

internal sealed class RefreshTokenCommandHandler(
    ICoinlyDbContext dbContext,
    ITokenProvider tokenProvider,
    IDateTimeService dateTimeService) : ICommandHandler<RefreshTokenCommand, IdentityToken>
{
    public async Task<Result<IdentityToken>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // Get user token by refresh token
        UserToken? userToken = await dbContext
            .UserTokens
            .SingleOrDefaultAsync(x => x.RefreshToken == request.RefreshToken, cancellationToken);

        if (userToken is null)
        {
            return Result.Fail<IdentityToken>(UserErrors.InvalidRefreshToken);
        }

        // Check if the refresh token is expired
        if (userToken.RefreshTokenExpiresOnUtc < dateTimeService.UtcNow)
        {
            return Result.Fail<IdentityToken>(UserErrors.RefreshTokenExpired);
        }

        // Get user with roles
        User? user = await dbContext
            .Users
            .Include(x => x.Roles)
            .SingleOrDefaultAsync(x => x.Id == userToken.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Fail<IdentityToken>(UserErrors.NotFound);
        }

        // Create new access token
        TokenRequest tokenRequest = new(
            user.Id.ToString(),
            user.Email,
            user.FirstName,
            user.LastName,
            user.Roles.Select(x => x.Name));

        var accessToken = tokenProvider.CreateToken(tokenRequest);

        // Update existing user token
        userToken.Update(
            accessToken.AccessToken,
            accessToken.AccessTokenExpiresOn,
            accessToken.RefreshToken,
            accessToken.RefreshTokenExpiresOn,
            dateTimeService.UtcNow);

        dbContext.UserTokens.Update(userToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        // Return new identity token
        return accessToken;
    }
}