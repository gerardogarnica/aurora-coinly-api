namespace Aurora.Coinly.Application.Abstractions.Authentication;

public sealed record IdentityToken(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresOn,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresOn)
{
    public override string ToString() =>
        $"IdentityToken {{ AccessToken = [REDACTED], AccessTokenExpiresOn = {AccessTokenExpiresOn}, RefreshToken = [REDACTED], RefreshTokenExpiresOn = {RefreshTokenExpiresOn} }}";
}
