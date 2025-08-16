namespace Aurora.Coinly.Domain.Users;

public sealed class UserToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string AccessToken { get; private set; }
    public DateTime AccessTokenExpiresOnUtc { get; private set; }
    public string RefreshToken { get; private set; }
    public DateTime RefreshTokenExpiresOnUtc { get; private set; }
    public DateTime IssuedOnUtc { get; private set; }
    public bool IsActive { get; private set; }
    public User User { get; init; } = null!;

    private UserToken() : base(Guid.NewGuid()) { }

    public static UserToken Create(
        Guid userId,
        string accessToken,
        DateTime accessTokenExpiresOnUtc,
        string refreshToken,
        DateTime refreshTokenExpiresOnUtc,
        DateTime issuedOnUtc)
    {
        return new UserToken
        {
            UserId = userId,
            AccessToken = accessToken,
            AccessTokenExpiresOnUtc = accessTokenExpiresOnUtc,
            RefreshToken = refreshToken,
            RefreshTokenExpiresOnUtc = refreshTokenExpiresOnUtc,
            IssuedOnUtc = issuedOnUtc,
            IsActive = true
        };
    }
}