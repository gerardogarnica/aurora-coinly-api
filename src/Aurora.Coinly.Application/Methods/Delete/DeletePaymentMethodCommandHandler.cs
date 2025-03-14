using Aurora.Coinly.Domain.Methods;

namespace Aurora.Coinly.Application.Methods.Delete;

internal sealed class DeletePaymentMethodCommandHandler(
    IPaymentMethodRepository paymentMethodRepository,
    IDateTimeService dateTimeService) : ICommandHandler<DeletePaymentMethodCommand>
{
    public async Task<Result> Handle(
        DeletePaymentMethodCommand request,
        CancellationToken cancellationToken)
    {
        // Get payment method
        var paymentMethod = await paymentMethodRepository.GetByIdAsync(request.Id);
        if (paymentMethod is null)
        {
            return Result.Fail<Guid>(PaymentMethodErrors.NotFound);
        }

        // Delete payment method
        var result = paymentMethod.Delete(dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        paymentMethodRepository.Update(paymentMethod);

        return Result.Ok();
    }
}