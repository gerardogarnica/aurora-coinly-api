namespace Aurora.Coinly.Application.Transactions.Remove;

public sealed record RemoveTransactionCommand(Guid Id) : ICommand;