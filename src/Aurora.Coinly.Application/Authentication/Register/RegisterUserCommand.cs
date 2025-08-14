using Aurora.Coinly.Application.Users;

namespace Aurora.Coinly.Application.Authentication.Register;

public sealed record RegisterUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Password) : ICommand<UserModel>;