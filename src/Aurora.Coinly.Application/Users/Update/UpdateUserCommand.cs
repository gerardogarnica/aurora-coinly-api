namespace Aurora.Coinly.Application.Users.Update;

public sealed record UpdateUserCommand(string FirstName, string LastName) : ICommand;