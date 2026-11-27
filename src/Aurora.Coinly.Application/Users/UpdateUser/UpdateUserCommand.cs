namespace Aurora.Coinly.Application.Users.UpdateUser;

public sealed record UpdateUserCommand(string FirstName, string LastName) : ICommand;
