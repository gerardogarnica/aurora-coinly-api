namespace Aurora.Coinly.Domain.Shared;

public static class ColorErrors
{
    public static readonly BaseError Empty = BaseError.Validation(
        "Color.Empty",
        "The color value cannot be empty");

    public static readonly BaseError InvalidFormat = BaseError.Validation(
        "Color.InvalidFormat",
        "The color must be a hex value like #RRGGBB");
}
