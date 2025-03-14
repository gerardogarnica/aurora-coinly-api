namespace Aurora.Coinly.Application.Methods.SetDefault;

public sealed record SetDefaultPaymentMethodCommand(Guid Id, bool IsDefault) : ICommand;