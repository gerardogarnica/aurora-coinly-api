using Aurora.Coinly.Domain.Users;

namespace Aurora.Coinly.Application.Authentication.Refresh;

internal sealed class RefreshTokenCommandHandler(
    IUserTokenRepository userTokenRepository,
    IUserRepository userRepository,
    ITokenProvider tokenProvider,
    IDateTimeService dateTimeService) : ICommandHandler<RefreshTokenCommand, IdentityToken>
{
    public async Task<Result<IdentityToken>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // Get user token by refresh token
        var userToken = await userTokenRepository.GetByRefreshTokenAsync(request.RefreshToken);
        if (userToken is null)
        {
            return Result.Fail<IdentityToken>(UserErrors.InvalidRefreshToken);
        }

        // Check if the refresh token is expired
        if (userToken.RefreshTokenExpiresOnUtc < dateTimeService.UtcNow)
        {
            return Result.Fail<IdentityToken>(UserErrors.RefreshTokenExpired);
        }

        // Create new access token
        var user = await userRepository.GetByIdAsync(userToken.UserId);
        if (user is null)
        {
            return Result.Fail<IdentityToken>(UserErrors.NotFound);
        }

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

        userTokenRepository.Update(userToken);

        // Return new identity token
        return accessToken;
    }
}