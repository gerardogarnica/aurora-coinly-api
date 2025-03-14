namespace Aurora.Coinly.Application.Summary.AddTransaction;

public sealed record AddTransactionToSummaryCommand(Guid TransactionId) : ICommand;