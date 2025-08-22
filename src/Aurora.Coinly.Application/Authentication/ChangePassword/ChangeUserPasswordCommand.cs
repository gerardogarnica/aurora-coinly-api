namespace Aurora.Coinly.Application.Authentication.ChangePassword;

public sealed record ChangeUserPasswordCommand(string CurrentPassword, string NewPassword) : ICommand;