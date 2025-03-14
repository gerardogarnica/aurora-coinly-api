namespace Aurora.Coinly.Application.Methods.Create;

public sealed record CreatePaymentMethodCommand(
    string Name,
    bool IsDefault,
    bool AllowRecurring,
    bool AutoMarkAsPaid,
    string? Notes) : ICommand<Guid>;