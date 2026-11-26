namespace Aurora.Coinly.Domain.Users;

public static class PasswordErrors
{
    public static readonly BaseError Required = BaseError.Validation(
        "Password.Required",
        "Password hash is required");

    public static readonly BaseError TooShort = BaseError.Validation(
        "Password.TooShort",
        $"Password hash must be at least {Password.MinHashLength} characters");

    public static readonly BaseError TooLong = BaseError.Validation(
        "Password.TooLong",
        $"Password hash cannot exceed {Password.MaxHashLength} characters");
}
