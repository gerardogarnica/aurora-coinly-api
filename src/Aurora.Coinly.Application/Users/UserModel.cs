namespace Aurora.Coinly.Application.Users;

public sealed record UserModel(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string FullName);

internal static class UserModelExtensions
{
    internal static UserModel ToModel(this User user) => new(
        user.Id,
        user.Email,
        user.FirstName,
        user.LastName,
        user.FullName);
}