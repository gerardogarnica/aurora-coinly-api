using Aurora.Coinly.Domain.UnitTests.Users;
using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Domain.UnitTests.Wallets;

internal static class WalletData
{
    public const string Name = "Name of the wallet";
    public static readonly Money AvailableAmount = new(100.0m, Currency.Usd);
    public const WalletType Type = WalletType.Cash;
    public const bool AllowNegative = false;
    public static readonly Color Color = Color.FromHex("#000000");
    public const string? Notes = "Notes of the category";

    public static Wallet GetWallet() => Wallet.Create(
        UserData.GetUser().Id,
        Name,
        AvailableAmount,
        Type,
        AllowNegative,
        Color,
        Notes,
        DateOnly.FromDateTime(DateTime.UtcNow),
        DateTime.UtcNow);
}