namespace Aurora.Coinly.Domain.Shared;

public static class ColorErrors
{
    public static readonly BaseError Required = BaseError.Validation(
        "Color.Required",
        "The color is required");

    public static readonly BaseError TooLong = BaseError.Validation(
        "Color.TooLong",
        $"The color cannot exceed {Color.MaxLength} characters");
}
