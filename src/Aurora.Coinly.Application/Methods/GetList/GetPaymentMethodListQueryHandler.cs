using Aurora.Coinly.Domain.Methods;

namespace Aurora.Coinly.Application.Methods.GetList;

internal sealed class GetPaymentMethodListQueryHandler(
    IPaymentMethodRepository paymentMethodRepository) : IQueryHandler<GetPaymentMethodListQuery, IReadOnlyCollection<PaymentMethodModel>>
{
    public async Task<Result<IReadOnlyCollection<PaymentMethodModel>>> Handle(
        GetPaymentMethodListQuery request,
        CancellationToken cancellationToken)
    {
        // Get payment methods
        var paymentMethods = await paymentMethodRepository.GetListAsync(request.ShowDeleted);

        // Return payment method models
        return paymentMethods.Select(x => x.ToModel()).ToList();
    }
}