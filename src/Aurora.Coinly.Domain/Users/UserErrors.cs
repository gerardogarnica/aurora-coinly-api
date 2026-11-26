namespace Aurora.Coinly.Domain.Users;

public static class UserErrors
{
    public static readonly BaseError NotFound = BaseError.NotFound(
        "User.NotFound",
        "The user with the specified identifier was not found");

    public static readonly BaseError FirstNameRequired = BaseError.Validation(
        "User.FirstNameRequired",
        "First name is required");

    public static readonly BaseError LastNameRequired = BaseError.Validation(
        "User.LastNameRequired",
        "Last name is required");

    public static readonly BaseError EmailAlreadyExists = BaseError.Conflict(
        "User.EmailAlreadyExists",
        "A user with the specified email address already exists");

    public static readonly BaseError Inactive = BaseError.Validation(
        "User.Inactive",
        "Cannot perform this operation on an inactive user");
}
