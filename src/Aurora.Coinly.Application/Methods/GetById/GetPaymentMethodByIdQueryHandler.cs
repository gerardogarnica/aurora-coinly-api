using Aurora.Coinly.Domain.Methods;

namespace Aurora.Coinly.Application.Methods.GetById;

internal sealed class GetPaymentMethodByIdQueryHandler(
    IPaymentMethodRepository paymentMethodRepository,
    IUserContext userContext) : IQueryHandler<GetPaymentMethodByIdQuery, PaymentMethodModel>
{
    public async Task<Result<PaymentMethodModel>> Handle(
        GetPaymentMethodByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Get payment method
        var paymentMethod = await paymentMethodRepository.GetByIdAsync(userContext.UserId, request.Id);
        if (paymentMethod is null)
        {
            return Result.Fail<PaymentMethodModel>(PaymentMethodErrors.NotFound);
        }

        // Return payment method model
        return paymentMethod.ToModel();
    }
}