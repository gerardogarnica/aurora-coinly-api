namespace Aurora.Coinly.Application.Authentication;

public sealed record IdentityToken(string AccessToken, string RefreshToken);