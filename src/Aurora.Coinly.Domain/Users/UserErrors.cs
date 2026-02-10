namespace Aurora.Coinly.Domain.Users;

public static class UserErrors
{
    public static readonly BaseError NotFound = BaseError.NotFound(
        "User.NotFound",
        "The user with the specified identifier was not found");

    public static readonly BaseError EmailAlreadyExists = BaseError.Conflict(
        "User.EmailAlreadyExists",
        "A user with the specified email address already exists");

    public static readonly BaseError InvalidCredentials = BaseError.Validation(
        "User.InvalidCredentials",
        "The provided credentials are invalid");

    public static readonly BaseError InvalidCurrentPassword = BaseError.Validation(
        "User.InvalidCurrentPassword",
        "The current password is invalid");

    public static readonly BaseError InvalidRefreshToken = BaseError.Validation(
        "User.InvalidRefreshToken",
        "The provided refresh token is invalid");

    public static readonly BaseError RefreshTokenExpired = BaseError.Validation(
        "User.RefreshTokenExpired",
        "The refresh token has expired");
}