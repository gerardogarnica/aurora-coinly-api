using System.Text.RegularExpressions;

namespace Aurora.Coinly.Domain.Shared;

public sealed partial record Color
{
    private static readonly Regex HexColorRegex = GenerateHexColorRegex();

    public string Value { get; init; }

    private Color(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Color value cannot be null or empty", nameof(value));
        }

        if (!HexColorRegex.IsMatch(value))
        {
            throw new ArgumentException("Color must be a hex value like #RRGGBB.");
        }

        Value = value.ToUpperInvariant();
    }

    public static Color FromHex(string hexValue) => new(hexValue);

    public static bool operator ==(Color left, string right) => left.Value == right;
    public static bool operator !=(Color left, string right) => !(left == right);

    public override string ToString() => Value;

    [GeneratedRegex(@"^#([0-9A-Fa-f]{6})$", RegexOptions.Compiled)]
    private static partial Regex GenerateHexColorRegex();
}