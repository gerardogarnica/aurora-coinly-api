namespace Aurora.Coinly.Domain.Users;

public sealed record Password : IValueObject
{
    private const int MinLength = 8;

    public string Value { get; init; }

    private Password(string value) => Value = value;

    public static Result<Password> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Fail<Password>(PasswordErrors.Empty);
        }

        if (value.Length < MinLength)
        {
            return Result.Fail<Password>(PasswordErrors.TooShort);
        }

        return new Password(value);
    }

    public override string ToString() => Value;
}