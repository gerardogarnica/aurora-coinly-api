using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Wallets.AssignToSavings;

internal sealed class AssignToSavingsCommandHandler(
    IWalletRepository walletRepository,
    IDateTimeService dateTimeService) : ICommandHandler<AssignToSavingsCommand>
{
    public async Task<Result> Handle(
        AssignToSavingsCommand request,
        CancellationToken cancellationToken)
    {
        // Get wallet
        var wallet = await walletRepository.GetByIdAsync(request.WalletId);
        if (wallet is null)
        {
            return Result.Fail(WalletErrors.NotFound);
        }

        // Assign to savings
        var result = wallet.AssignToSavings(
            new Money(request.Amount, wallet.TotalAmount.Currency),
            request.AssignedOn,
            dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        walletRepository.Update(wallet);

        return Result.Ok();
    }
}