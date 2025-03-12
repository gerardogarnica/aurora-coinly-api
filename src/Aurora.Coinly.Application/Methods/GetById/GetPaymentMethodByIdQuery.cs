namespace Aurora.Coinly.Application.Methods.GetById;

public sealed record GetPaymentMethodByIdQuery(Guid Id) : IQuery<PaymentMethodModel>;