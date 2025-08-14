namespace Aurora.Coinly.Domain.UnitTests.Users;

internal static class UserData
{
    public const string Email = "test@test.mail";
    public const string FirstName = "First";
    public const string LastName = "Last";
    public const string Password = "test123";

    public static User GetUser() => User.Create(
        Email,
        FirstName,
        LastName,
        Password,
        Guid.NewGuid().ToString(),
        DateTime.UtcNow);
}