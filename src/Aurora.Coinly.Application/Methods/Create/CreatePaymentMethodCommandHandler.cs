namespace Aurora.Coinly.Application.Methods.Create;

internal sealed class CreatePaymentMethodCommandHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<CreatePaymentMethodCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreatePaymentMethodCommand request,
        CancellationToken cancellationToken)
    {
        // Get wallet
        Wallet? wallet = await dbContext
            .Wallets
            .Include(x => x.Methods)
            .SingleOrDefaultAsync(x => x.Id == request.WalletId && x.UserId == userContext.UserId, cancellationToken);

        if (wallet is null)
        {
            return Result.Fail<Guid>(WalletErrors.NotFound);
        }

        if (wallet.IsDeleted)
        {
            return Result.Fail<Guid>(WalletErrors.IsDeleted);
        }

        // Get other payment methods
        var markAsDefault = await IsSetAsDefault(request.IsDefault, cancellationToken);

        // Create payment method
        var paymentMethod = PaymentMethod.Create(
            userContext.UserId,
            request.Name,
            markAsDefault,
            request.AllowRecurring,
            request.AutoMarkAsPaid,
            wallet,
            request.MaxDaysToReverse,
            request.SuggestedPaymentDay,
            request.StatementCutoffDay,
            request.Notes,
            dateTimeService.UtcNow);

        dbContext.PaymentMethods.Add(paymentMethod);

        await dbContext.SaveChangesAsync(cancellationToken);

        return paymentMethod.Id;
    }

    private async Task<bool> IsSetAsDefault(bool isDefault, CancellationToken cancellationToken)
    {
        List<PaymentMethod> methods = await dbContext
            .PaymentMethods
            .Where(x => x.UserId == userContext.UserId)
            .ToListAsync(cancellationToken);

        if (!methods.Any())
        {
            return true;
        }

        if (methods.Any(x => x.IsDefault) && isDefault)
        {
            var defaultMethod = methods.First(x => x.IsDefault);
            defaultMethod.SetAsNotDefault(dateTimeService.UtcNow);

            dbContext.PaymentMethods.Update(defaultMethod);
        }

        return isDefault;
    }
}