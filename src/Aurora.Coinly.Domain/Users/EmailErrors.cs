namespace Aurora.Coinly.Domain.Users;

public static class EmailErrors
{
    public static readonly BaseError Empty = BaseError.Validation(
        "Email.Empty",
        "The email is required");

    public static readonly BaseError TooLong = BaseError.Validation(
        "Email.TooLong",
        $"The email cannot exceed {Email.MaxLength} characters");

    public static readonly BaseError InvalidFormat = BaseError.Validation(
        "Email.InvalidFormat",
        "The email format is invalid");
}
