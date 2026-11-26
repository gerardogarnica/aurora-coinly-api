using System.Net.Mail;

namespace Aurora.Coinly.Domain.Users;

public sealed record Email : IValueObject
{
    public const int MaxLength = 255;

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Fail<Email>(EmailErrors.Empty);
        }

        email = email.Trim().ToLowerInvariant();

        if (email.Length > MaxLength)
        {
            return Result.Fail<Email>(EmailErrors.TooLong);
        }

        if (!IsValidFormat(email))
        {
            return Result.Fail<Email>(EmailErrors.InvalidFormat);
        }

        return new Email(email);
    }

    private static bool IsValidFormat(string email)
    {
        if (!MailAddress.TryCreate(email, out MailAddress? address))
        {
            return false;
        }

        string host = address.Host;

        return address.Address == email
            && host.Contains('.', StringComparison.Ordinal)
            && !host.EndsWith('.');
    }

    public static implicit operator string(Email email) => email.Value;

    public override string ToString() => Value;
}
