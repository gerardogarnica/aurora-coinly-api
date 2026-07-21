namespace Aurora.Coinly.Application.Authentication.ChangePassword;

public sealed record ChangeUserPasswordCommand(string CurrentPassword, string NewPassword) : ICommand
{
#pragma warning disable S2068 // Not a credential: redacted placeholder for logging, never a real password.
    public override string ToString() =>
        "ChangeUserPasswordCommand { CurrentPassword = [REDACTED], NewPassword = [REDACTED] }";
#pragma warning restore S2068
}