namespace Aurora.Coinly.Domain.Categories;

public static class CategoryErrors
{
    public static readonly BaseError NotFound = new(
        "Category.Found",
        "The category with the specified identifier was not found");

    public static readonly BaseError InvalidType = new(
        "Category.InvalidType",
        "The category type is not valid");

    public static readonly BaseError IsDeleted = new(
        "Category.IsDeleted",
        "The category is already deleted");
}