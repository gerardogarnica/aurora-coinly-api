using Aurora.Coinly.Application.Users;

namespace Aurora.Coinly.Application.Authentication.Register;

public sealed record RegisterUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Password) : ICommand<UserModel>
{
#pragma warning disable S2068 // Not a credential: redacted placeholder for logging, never a real password.
    public override string ToString() =>
        $"RegisterUserCommand {{ Email = {Email}, FirstName = {FirstName}, LastName = {LastName}, Password = [REDACTED] }}";
#pragma warning restore S2068
}