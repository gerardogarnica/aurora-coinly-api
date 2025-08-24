using Aurora.Coinly.Domain.Methods;
using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Methods.Update;

internal sealed class UpdatePaymentMethodCommandHandler(
    ICoinlyDbContext dbContext,
    IPaymentMethodRepository paymentMethodRepository,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<UpdatePaymentMethodCommand>
{
    public async Task<Result> Handle(
        UpdatePaymentMethodCommand request,
        CancellationToken cancellationToken)
    {
        // Get payment method
        var paymentMethod = await paymentMethodRepository.GetByIdAsync(userContext.UserId, request.Id);
        if (paymentMethod is null)
        {
            return Result.Fail<Guid>(PaymentMethodErrors.NotFound);
        }

        // Get wallet
        Wallet? wallet = await dbContext
            .Wallets
            .Include(x => x.Methods)
            .SingleOrDefaultAsync(x => x.Id == request.WalletId && x.UserId == userContext.UserId, cancellationToken);

        if (wallet is null)
        {
            return Result.Fail<Guid>(WalletErrors.NotFound);
        }

        // Update payment method
        var result = paymentMethod.Update(
            request.Name,
            request.AllowRecurring,
            request.AutoMarkAsPaid,
            wallet,
            request.MaxDaysToReverse,
            request.SuggestedPaymentDay,
            request.StatementCutoffDay,
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