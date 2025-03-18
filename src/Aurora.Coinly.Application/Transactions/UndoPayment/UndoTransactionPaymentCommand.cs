namespace Aurora.Coinly.Application.Transactions.UndoPayment;

public sealed record UndoTransactionPaymentCommand(Guid Id) : ICommand;