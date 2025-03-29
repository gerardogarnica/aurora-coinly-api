using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Domain.UnitTests.Wallets;

internal static class WalletData
{
    public const string Name = "Name of the wallet";
    public static readonly Money AvailableAmount = new(10.0m, Currency.Usd);
    public const WalletType Type = WalletType.Cash;
    public const string? Notes = "Notes of the category";

    public static Wallet GetWallet() => Wallet.Create(
        Name,
        AvailableAmount,
        Type,
        Notes,
        DateTime.UtcNow);
}