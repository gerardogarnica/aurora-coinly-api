namespace Aurora.Coinly.Application.Methods.GetById;

internal sealed class GetPaymentMethodByIdQueryHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext) : IQueryHandler<GetPaymentMethodByIdQuery, PaymentMethodModel>
{
    public async Task<Result<PaymentMethodModel>> Handle(
        GetPaymentMethodByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Get payment method
        PaymentMethod? paymentMethod = await dbContext
            .PaymentMethods
            .Include(x => x.Wallet)
            .SingleOrDefaultAsync(x => x.Id == request.Id && x.UserId == userContext.UserId, cancellationToken);

        if (paymentMethod is null)
        {
            return Result.Fail<PaymentMethodModel>(PaymentMethodErrors.NotFound);
        }

        // Return payment method model
        return paymentMethod.ToModel();
    }
}