namespace Aurora.Coinly.Domain.Users;

public sealed record Password
{
    private const int MinLength = 8;

    public string Value { get; init; }

    private Password(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Password cannot be null or empty", nameof(value));
        }

        if (value.Length < MinLength)
        {
            throw new ArgumentException($"Password must be at least {MinLength} characters long", nameof(value));
        }

        Value = value;
    }

    public static Password Create(string value) => new(value);

    public override string ToString() => Value;
}