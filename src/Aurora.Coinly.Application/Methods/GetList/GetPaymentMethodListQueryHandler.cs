namespace Aurora.Coinly.Application.Methods.GetList;

internal sealed class GetPaymentMethodListQueryHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext) : IQueryHandler<GetPaymentMethodListQuery, IReadOnlyCollection<PaymentMethodModel>>
{
    public async Task<Result<IReadOnlyCollection<PaymentMethodModel>>> Handle(
        GetPaymentMethodListQuery request,
        CancellationToken cancellationToken)
    {
        // Get payment methods
        var query = dbContext
           .PaymentMethods
           .Include(x => x.Wallet)
           .Where(x => x.UserId == userContext.UserId)
           .AsNoTracking()
           .AsQueryable();

        if (!request.ShowDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        List<PaymentMethod> paymentMethods = await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);

        // Return payment method models
        return paymentMethods.Select(x => x.ToModel()).ToList();
    }
}