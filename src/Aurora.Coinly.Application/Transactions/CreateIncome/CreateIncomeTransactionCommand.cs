namespace Aurora.Coinly.Application.Transactions.CreateIncome;

public sealed record CreateIncomeTransactionCommand(
    Guid CategoryId,
    string Description,
    DateOnly TransactionDate,
    string CurrencyCode,
    decimal Amount,
    string? Notes,
    Guid WalletId) : ICommand<Guid>;