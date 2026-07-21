namespace Aurora.Coinly.Application.Authentication.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<IdentityToken>
{
#pragma warning disable S2068 // Not a credential: redacted placeholder for logging, never a real password.
    public override string ToString() => $"LoginCommand {{ Email = {Email}, Password = [REDACTED] }}";
#pragma warning restore S2068
}