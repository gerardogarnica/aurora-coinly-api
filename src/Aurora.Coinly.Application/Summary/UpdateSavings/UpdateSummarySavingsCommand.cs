namespace Aurora.Coinly.Application.Summary.UpdateSavings;

public sealed record UpdateSummarySavingsCommand(
    Guid WalletId,
    Guid UserId,
    Money Amount,
    DateOnly AssignedOn,
    bool IsIncrement) : ICommand;