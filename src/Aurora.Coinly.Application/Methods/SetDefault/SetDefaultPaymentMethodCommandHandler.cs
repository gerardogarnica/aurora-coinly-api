namespace Aurora.Coinly.Application.Methods.SetDefault;

internal sealed class SetDefaultPaymentMethodCommandHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<SetDefaultPaymentMethodCommand>
{
    public async Task<Result> Handle(
        SetDefaultPaymentMethodCommand request,
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

        // Get existing default method
        var defaultMethod = await dbContext
            .PaymentMethods
            .SingleOrDefaultAsync(x => x.UserId == userContext.UserId && x.IsDefault && !x.IsDeleted, cancellationToken);

        if (defaultMethod is not null && defaultMethod.Id != paymentMethod.Id)
        {
            defaultMethod.SetAsNotDefault(dateTimeService.UtcNow);
            dbContext.PaymentMethods.Update(defaultMethod);
        }

        // Set as default
        var result = paymentMethod.SetAsDefault(dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        dbContext.PaymentMethods.Update(paymentMethod);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}