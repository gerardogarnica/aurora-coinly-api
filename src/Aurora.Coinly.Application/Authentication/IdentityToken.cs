namespace Aurora.Coinly.Application.Authentication;

public sealed record IdentityToken(
    string AccessToken,
    DateTime AccessTokenExpiresOn,
    string RefreshToken,
    DateTime RefreshTokenExpiresOn);