using Aurora.Coinly.Domain.Users;

namespace Aurora.Coinly.Application.Authentication.Login;

internal sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IUserTokenRepository userTokenRepository,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider,
    IDateTimeService dateTimeService) : ICommandHandler<LoginCommand, IdentityToken>
{
    public async Task<Result<IdentityToken>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // Get user by email
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user is null)
        {
            return Result.Fail<IdentityToken>(UserErrors.InvalidCredentials);
        }

        // Validate password
        var password = Password.Create(request.Password);
        if (!user.VerifyPassword(password, passwordHasher))
        {
            return Result.Fail<IdentityToken>(UserErrors.InvalidCredentials);
        }

        // Create token
        TokenRequest tokenRequest = new(
            user.Id.ToString(),
            user.Email,
            user.FirstName,
            user.LastName,
            user.Roles.Select(x => x.Name));

        var accessToken = tokenProvider.CreateToken(tokenRequest);

        // Create user token
        var userToken = UserToken.Create(
            user.Id,
            accessToken.AccessToken,
            accessToken.AccessTokenExpiresOn,
            accessToken.RefreshToken,
            accessToken.RefreshTokenExpiresOn,
            dateTimeService.UtcNow);

        await userTokenRepository.AddAsync(userToken, cancellationToken);

        // Return identity token
        return accessToken;
    }
}