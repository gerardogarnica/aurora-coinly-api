namespace Aurora.Coinly.Domain.Users;

public sealed record Password : IValueObject
{
    public const int MinHashLength = 8;
    public const int MaxHashLength = 500;

    public string Hash { get; init; }

    private Password(string hash) => Hash = hash;

    public static Result<Password> Create(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            return Result.Fail<Password>(PasswordErrors.Required);
        }

        if (hash.Length < MinHashLength)
        {
            return Result.Fail<Password>(PasswordErrors.TooShort);
        }

        if (hash.Length > MaxHashLength)
        {
            return Result.Fail<Password>(PasswordErrors.TooLong);
        }

        return new Password(hash);
    }

    public override string ToString() => Hash;
}
