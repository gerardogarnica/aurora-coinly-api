using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Wallets.Delete;

internal sealed class DeleteWalletCommandHandler(
    IWalletRepository walletRepository,
    IDateTimeService dateTimeService) : ICommandHandler<DeleteWalletCommand>
{
    public async Task<Result> Handle(
        DeleteWalletCommand request,
        CancellationToken cancellationToken)
    {
        // Get wallet
        var wallet = await walletRepository.GetByIdAsync(request.Id);
        if (wallet is null)
        {
            return Result.Fail(WalletErrors.NotFound);
        }

        // Update wallet
        var result = wallet.Delete(dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        walletRepository.Update(wallet);

        return Result.Ok();
    }
}