using System.Text.RegularExpressions;

namespace Aurora.Coinly.Domain.Shared;

public sealed partial record Color : IValueObject
{
    private static readonly Regex HexColorRegex = GenerateHexColorRegex();

    public string Value { get; init; }

    // EF Core materializes Color through this constructor; validation lives in FromHex.
    private Color(string value) => Value = value;

    public static Result<Color> FromHex(string hexValue)
    {
        if (string.IsNullOrWhiteSpace(hexValue))
        {
            return Result.Fail<Color>(ColorErrors.Empty);
        }

        if (!HexColorRegex.IsMatch(hexValue))
        {
            return Result.Fail<Color>(ColorErrors.InvalidFormat);
        }

        return new Color(hexValue.ToUpperInvariant());
    }

    public static bool operator ==(Color left, string right) => left.Value == right;
    public static bool operator !=(Color left, string right) => !(left == right);

    public override string ToString() => Value;

    [GeneratedRegex(@"^#([0-9A-Fa-f]{6})$", RegexOptions.Compiled)]
    private static partial Regex GenerateHexColorRegex();
}