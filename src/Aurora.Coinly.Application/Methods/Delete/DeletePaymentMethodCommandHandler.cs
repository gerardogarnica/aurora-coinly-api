namespace Aurora.Coinly.Application.Methods.Delete;

internal sealed class DeletePaymentMethodCommandHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<DeletePaymentMethodCommand>
{
    public async Task<Result> Handle(
        DeletePaymentMethodCommand request,
        CancellationToken cancellationToken)
    {
        // Get payment method
        PaymentMethod? paymentMethod = await dbContext
            .PaymentMethods
            .SingleOrDefaultAsync(x => x.Id == request.Id && x.UserId == userContext.UserId, cancellationToken);

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

        dbContext.PaymentMethods.Update(paymentMethod);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}