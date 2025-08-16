using Aurora.Coinly.Domain.Users;

namespace Aurora.Coinly.Application.Authentication.Refresh;

internal sealed class RefreshTokenCommandHandler(
    IUserTokenRepository userTokenRepository,
    ITokenProvider tokenProvider,
    IDateTimeService dateTimeService) : ICommandHandler<RefreshTokenCommand, IdentityToken>
{
    public async Task<Result<IdentityToken>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // Get user token by refresh token
        var userToken = await userTokenRepository.GetByRefreshTokenAsync(request.RefreshToken);
        if (userToken is null || !userToken.IsActive)
        {
            return Result.Fail<IdentityToken>(UserErrors.InvalidRefreshToken);
        }

        // Check if the refresh token is expired
        if (userToken.RefreshTokenExpiresOnUtc < dateTimeService.UtcNow)
        {
            return Result.Fail<IdentityToken>(UserErrors.RefreshTokenExpired);
        }

        // Create new access token
        TokenRequest tokenRequest = new(
            userToken.UserId.ToString(),
            userToken.User.Email,
            userToken.User.FirstName,
            userToken.User.LastName,
            userToken.User.Roles.Select(x => x.Name));

        var accessToken = tokenProvider.CreateToken(tokenRequest);

        // Create new user token
        var newUserToken = UserToken.Create(
            userToken.UserId,
            accessToken.AccessToken,
            accessToken.AccessTokenExpiresOn,
            accessToken.RefreshToken,
            accessToken.RefreshTokenExpiresOn,
            dateTimeService.UtcNow);

        await userTokenRepository.AddAsync(newUserToken, cancellationToken);

        // Return new identity token
        return accessToken;
    }
}