namespace Aurora.Coinly.Domain.Categories;

public static class CategoryErrors
{
    public static readonly BaseError NotFound = BaseError.NotFound(
        "Category.Found",
        "The category with the specified identifier was not found");

    public static readonly BaseError InvalidType = BaseError.Validation(
        "Category.InvalidType",
        "The category type is not valid");

    public static readonly BaseError IsDeleted = BaseError.Validation(
        "Category.IsDeleted",
        "The category is already deleted");

    public static readonly BaseError IsUnavailableToReverse = BaseError.Validation(
        "Category.IsUnavailableToReverse",
        "The category is unavailable to reverse the transaction");
}