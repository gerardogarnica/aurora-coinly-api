using Aurora.Coinly.Domain.Methods;

namespace Aurora.Coinly.Application.Methods.Update;

internal sealed class UpdatePaymentMethodCommandHandler(
    IPaymentMethodRepository paymentMethodRepository,
    IDateTimeService dateTimeService) : ICommandHandler<UpdatePaymentMethodCommand>
{
    public async Task<Result> Handle(
        UpdatePaymentMethodCommand request,
        CancellationToken cancellationToken)
    {
        // Get payment method
        var paymentMethod = await paymentMethodRepository.GetByIdAsync(request.Id);
        if (paymentMethod is null)
        {
            return Result.Fail<Guid>(PaymentMethodErrors.NotFound);
        }

        // Update payment method
        var result = paymentMethod.Update(
            request.Name,
            request.AllowRecurring,
            request.AutoMarkAsPaid,
            request.Notes,
            dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        paymentMethodRepository.Update(paymentMethod);

        return Result.Ok();
    }
}