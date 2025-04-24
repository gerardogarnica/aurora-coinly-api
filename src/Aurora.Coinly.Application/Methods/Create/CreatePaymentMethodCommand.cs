namespace Aurora.Coinly.Application.Methods.Create;

public sealed record CreatePaymentMethodCommand(
    string Name,
    bool IsDefault,
    bool AllowRecurring,
    bool AutoMarkAsPaid,
    Guid RelatedWalletId,
    int? SuggestedPaymentDay,
    int? StatementCutoffDay,
    string? Notes) : ICommand<Guid>;