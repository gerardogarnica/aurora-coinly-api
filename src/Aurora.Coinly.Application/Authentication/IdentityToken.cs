namespace Aurora.Coinly.Application.Authentication;

public sealed record IdentityToken(
    string AccessToken,
    DateTime AccessTokenExpiresOn,
    string RefreshToken,
    DateTime RefreshTokenExpiresOn)
{
    public override string ToString() =>
        $"IdentityToken {{ AccessToken = [REDACTED], AccessTokenExpiresOn = {AccessTokenExpiresOn}, RefreshToken = [REDACTED], RefreshTokenExpiresOn = {RefreshTokenExpiresOn} }}";
}