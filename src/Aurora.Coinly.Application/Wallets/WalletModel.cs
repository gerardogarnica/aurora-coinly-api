namespace Aurora.Coinly.Application.Wallets;

public sealed record WalletModel(
    Guid WalletId,
    string Name,
    string CurrencyCode,
    decimal AvailableAmount,
    decimal SavingsAmount,
    decimal TotalAmount,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    WalletType Type,
    bool AllowNegative,
    string Color,
    bool CanDelete,
    bool IsDeleted,
    string? Notes,
    DateOnly OpenedOn,
    DateOnly LastOperationOn,
    List<WalletTransactionModel> Transactions);

internal static class WalletModelExtensions
{
    internal static WalletModel ToModel(this Wallet wallet) => new(
        wallet.Id,
        wallet.Name,
        wallet.TotalAmount.Currency.Code,
        wallet.AvailableAmount.Amount,
        wallet.SavingsAmount.Amount,
        wallet.TotalAmount.Amount,
        wallet.Type,
        wallet.AllowNegative,
        wallet.Color.Value,
        !wallet.Methods.Any(x => !x.IsDeleted) && !wallet.IsDeleted,
        wallet.IsDeleted,
        wallet.Notes,
        wallet.OpenedOn,
        wallet.LastOperationOn,
        [.. wallet.Operations.Select(x => x.ToModel()).OrderBy(x => x.Date)]);
}