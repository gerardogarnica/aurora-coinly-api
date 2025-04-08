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

        // Get existing default method
        var methods = await paymentMethodRepository.GetListAsync(true);

        var defaultMethod = methods.First(x => x.IsDefault);
        if (defaultMethod is not null && defaultMethod.Id != paymentMethod.Id)
        {
            defaultMethod.SetAsNotDefault(dateTimeService.UtcNow);
            paymentMethodRepository.Update(defaultMethod);
        }

        // Set as default
        var result = paymentMethod.SetAsDefault(dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        paymentMethodRepository.Update(paymentMethod);

        return Result.Ok();
    }
}