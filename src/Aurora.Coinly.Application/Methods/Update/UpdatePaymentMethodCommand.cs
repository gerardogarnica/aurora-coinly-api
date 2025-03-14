namespace Aurora.Coinly.Application.Methods.Update;

public sealed record UpdatePaymentMethodCommand(
    Guid Id,
    string Name,
    bool AllowRecurring,
    bool AutoMarkAsPaid,
    string? Notes) : ICommand;