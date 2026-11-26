namespace Aurora.Coinly.Application.Abstractions.Authentication;

public sealed record IssuedToken(
    IdentityToken Identity,
    string AccessTokenId,
    string RefreshTokenHash);
