namespace Aurora.Coinly.Application.Methods.GetList;

public sealed record GetPaymentMethodListQuery(bool ShowDeleted) : IQuery<IReadOnlyCollection<PaymentMethodModel>>;