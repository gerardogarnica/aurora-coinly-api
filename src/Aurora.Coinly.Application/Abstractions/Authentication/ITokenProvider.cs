using Aurora.Coinly.Application.Authentication;

namespace Aurora.Coinly.Application.Abstractions.Authentication;

public interface ITokenProvider
{
    IdentityToken CreateToken(TokenRequest tokenRequest);
}