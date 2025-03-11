using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Wallets;

public sealed record WalletTransactionModel(
    Guid OperationId,
    WalletHistoryType Type,
    string Description,
    decimal Amount,
    decimal AvailableBalance,
    decimal SavingsBalance,
    DateOnly Date);

internal static class WalletTransactionModelExtensions
{
    internal static WalletTransactionModel ToModel(this WalletHistory walletHistory) => new(
        walletHistory.Id,
        walletHistory.Type,
        walletHistory.Description,
        walletHistory.IsIncrement ? walletHistory.Amount.Amount : -walletHistory.Amount.Amount,
        walletHistory.AvailableBalance.Amount,
        walletHistory.SavingsBalance.Amount,
        walletHistory.Date);
}