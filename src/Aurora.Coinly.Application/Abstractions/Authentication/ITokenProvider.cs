namespace Aurora.Coinly.Application.Abstractions.Authentication;

public interface ITokenProvider
{
    IssuedToken CreateToken(TokenRequest tokenRequest);

    string HashRefreshToken(string refreshToken);
}
