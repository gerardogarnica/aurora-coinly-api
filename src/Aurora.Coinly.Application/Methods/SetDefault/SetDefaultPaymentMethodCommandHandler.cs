using Aurora.Coinly.Domain.Methods;

namespace Aurora.Coinly.Application.Methods.SetDefault;

internal sealed class SetDefaultPaymentMethodCommandHandler(
    IPaymentMethodRepository paymentMethodRepository,
    IDateTimeService dateTimeService) : ICommandHandler<SetDefaultPaymentMethodCommand>
{
    public async Task<Result> Handle(
        SetDefaultPaymentMethodCommand request,
        CancellationToken cancellationToken)
    {
        // Get payment method
        var paymentMethod = await paymentMethodRepository.GetByIdAsync(request.Id);
        if (paymentMethod is null)
        {
            return Result.Fail<Guid>(PaymentMethodErrors.NotFound);
        }

        // Set as default or not
        var result = request.IsDefault
            ? paymentMethod.SetAsDefault(dateTimeService.UtcNow)
            : paymentMethod.SetAsNotDefault(dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        paymentMethodRepository.Update(paymentMethod);

        return Result.Ok();
    }
}