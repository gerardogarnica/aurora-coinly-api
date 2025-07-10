namespace Aurora.Coinly.Domain.Users;

public static class UserErrors
{
    public static readonly BaseError NotFound = new(
        "User.NotFound",
        "The user with the specified identifier was not found");
}