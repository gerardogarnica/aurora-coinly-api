namespace Aurora.Coinly.Application.Summary.AddTransaction;

public sealed record AddSummaryTransactionCommand(Guid TransactionId) : ICommand;