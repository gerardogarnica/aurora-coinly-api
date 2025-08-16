namespace Aurora.Coinly.Domain.Users;

public static class UserErrors
{
    public static readonly BaseError NotFound = new(
        "User.NotFound",
        "The user with the specified identifier was not found");

    public static readonly BaseError EmailAlreadyExists = new(
        "User.EmailAlreadyExists",
        "A user with the specified email address already exists");

    public static readonly BaseError InvalidCredentials = new(
        "User.InvalidCredentials",
        "The provided credentials are invalid");

    public static readonly BaseError InvalidRefreshToken = new(
        "User.InvalidRefreshToken",
        "The provided refresh token is invalid");

    public static readonly BaseError RefreshTokenExpired = new(
        "User.RefreshTokenExpired",
        "The refresh token has expired");
}