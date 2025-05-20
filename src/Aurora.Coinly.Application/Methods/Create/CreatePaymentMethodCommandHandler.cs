using Aurora.Coinly.Domain.Methods;
using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Methods.Create;

internal sealed class CreatePaymentMethodCommandHandler(
    IPaymentMethodRepository paymentMethodRepository,
    IWalletRepository walletRepository,
    IDateTimeService dateTimeService) : ICommandHandler<CreatePaymentMethodCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreatePaymentMethodCommand request,
        CancellationToken cancellationToken)
    {
        // Get wallet
        var wallet = await walletRepository.GetByIdAsync(request.WalletId);
        if (wallet is null)
        {
            return Result.Fail<Guid>(WalletErrors.NotFound);
        }

        if (wallet.IsDeleted)
        {
            return Result.Fail<Guid>(WalletErrors.IsDeleted);
        }

        // Get other payment methods
        var markAsDefault = await IsSetAsDefault(request.IsDefault);

        // Create payment method
        var paymentMethod = PaymentMethod.Create(
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

        await paymentMethodRepository.AddAsync(paymentMethod, cancellationToken);

        return paymentMethod.Id;
    }

    private async Task<bool> IsSetAsDefault(bool isDefault)
    {
        var methods = await paymentMethodRepository.GetListAsync(true);
        if (!methods.Any())
        {
            return true;
        }

        if (methods.Any(x => x.IsDefault) && isDefault)
        {
            var defaultMethod = methods.First(x => x.IsDefault);
            defaultMethod.SetAsNotDefault(dateTimeService.UtcNow);

            paymentMethodRepository.Update(defaultMethod);
        }

        return isDefault;
    }
}