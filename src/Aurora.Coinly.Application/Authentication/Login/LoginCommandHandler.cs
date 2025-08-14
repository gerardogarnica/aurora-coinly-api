using Aurora.Coinly.Domain.Users;

namespace Aurora.Coinly.Application.Authentication.Login;

internal sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider) : ICommandHandler<LoginCommand, IdentityToken>
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

        // Return token
        return accessToken;
    }
}