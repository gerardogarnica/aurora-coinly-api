using Newtonsoft.Json;

namespace Aurora.Coinly.Domain.Shared;

public sealed record Currency
{
    internal static readonly Currency Default = new("");
    public static readonly Currency Usd = new("USD");
    public static readonly Currency Eur = new("EUR");

    public string Code { get; init; }

    public static readonly IReadOnlyCollection<Currency> All =
    [
        Usd,
        Eur
    ];

    [JsonConstructor]
    public Currency() { }

    private Currency(string code) => Code = code;

    public static Currency FromCode(string code)
    {
        return All.FirstOrDefault(c => c.Code == code) ?? Default;
    }
}