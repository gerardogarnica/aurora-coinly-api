namespace Aurora.Coinly.Domain.UnitTests.Users;

internal static class UserData
{
    public static readonly Email Email = Email.Create("test@test.mail").Value;
    public static readonly Password Password = Password.Create("hashed-password").Value;
    public const string FirstName = "First";
    public const string LastName = "Last";

    public static readonly DateTime CreatedOnUtc = new(2026, 1, 10, 9, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime IssuedOnUtc = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime AccessTokenExpiresOnUtc = IssuedOnUtc.AddMinutes(15);
    public static readonly DateTime RefreshTokenExpiresOnUtc = IssuedOnUtc.AddDays(7);

    public const string AccessTokenId = "access-token-id";

    public static User GetUser() => User.Create(
        FirstName,
        LastName,
        Email,
        Password,
        CreatedOnUtc).Value;

    public static string NewRefreshTokenHash() => Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

    public static UserToken IssueToken(User user) => user.IssueToken(
        AccessTokenId,
        NewRefreshTokenHash(),
        AccessTokenExpiresOnUtc,
        RefreshTokenExpiresOnUtc,
        IssuedOnUtc).Value;

    public static UserToken IssueToken(User user, DateTime refreshTokenExpiresOnUtc) => user.IssueToken(
        AccessTokenId,
        NewRefreshTokenHash(),
        AccessTokenExpiresOnUtc,
        refreshTokenExpiresOnUtc,
        IssuedOnUtc).Value;
}
