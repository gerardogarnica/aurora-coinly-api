namespace Aurora.Coinly.Application.Methods.Delete;

public sealed record DeletePaymentMethodCommand(Guid Id) : ICommand;