namespace Aurora.Coinly.Application.Authentication.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<IdentityToken>
{
    public override string ToString() => "RefreshTokenCommand { RefreshToken = [REDACTED] }";
}