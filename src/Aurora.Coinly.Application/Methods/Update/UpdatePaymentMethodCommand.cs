namespace Aurora.Coinly.Application.Methods.Update;

public sealed record UpdatePaymentMethodCommand(
    Guid Id,
    string Name,
    bool AllowRecurring,
    bool AutoMarkAsPaid,
    Guid WalletId,
    int MaxDaysToReverse,
    int? SuggestedPaymentDay,
    int? StatementCutoffDay,
    string? Notes) : ICommand;