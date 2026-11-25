namespace Aurora.Coinly.Domain.Users;

public static class PasswordErrors
{
    public static readonly BaseError Empty = BaseError.Validation(
        "Password.Empty",
        "The password cannot be empty");

    public static readonly BaseError TooShort = BaseError.Validation(
        "Password.TooShort",
        "The password must be at least 8 characters long");
}
