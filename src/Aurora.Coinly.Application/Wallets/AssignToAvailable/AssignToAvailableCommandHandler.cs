using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Wallets.AssignToAvailable;

internal sealed class AssignToAvailableCommandHandler(
    IWalletRepository walletRepository,
    IDateTimeService dateTimeService) : ICommandHandler<AssignToAvailableCommand>
{
    public async Task<Result> Handle(
        AssignToAvailableCommand request,
        CancellationToken cancellationToken)
    {
        // Get wallet
        var wallet = await walletRepository.GetByIdAsync(request.WalletId);
        if (wallet is null)
        {
            return Result.Fail(WalletErrors.NotFound);
        }

        // Assign to available
        var result = wallet.AssignToAvailable(
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