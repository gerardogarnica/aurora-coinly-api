namespace Aurora.Coinly.Infrastructure.Authentication;

public sealed class JwtAuthOptions
{
    public const int MinimumKeyLengthInBytes = 32;
    public const string SectionName = "Jwt";

    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string Key { get; init; }
    public int LifeTimeInMinutes { get; init; }
    public int RefreshTokenExpirationDays { get; init; }
}