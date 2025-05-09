﻿using Aurora.Coinly.Domain.Wallets;

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
    bool IsDeleted,
    string? Notes,
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
        wallet.IsDeleted,
        wallet.Notes,
        [.. wallet.Operations.Select(x => x.ToModel()).OrderBy(x => x.Date)]);
}