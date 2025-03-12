using Aurora.Coinly.Domain.Methods;

namespace Aurora.Coinly.Application.Methods.Create;

internal sealed class CreatePaymentMethodCommandHandler(
    IPaymentMethodRepository paymentMethodRepository,
    IDateTimeService dateTimeService) : ICommandHandler<CreatePaymentMethodCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreatePaymentMethodCommand request,
        CancellationToken cancellationToken)
    {
        // Create payment method
        var paymentMethod = PaymentMethod.Create(
            request.Name,
            request.IsDefault,
            request.AllowRecurring,
            request.AutoMarkAsPaid,
            request.Notes,
            dateTimeService.UtcNow);

        await paymentMethodRepository.AddAsync(paymentMethod, cancellationToken);

        return paymentMethod.Id;
    }
}