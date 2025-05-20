using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Wallets.Update;

internal sealed class UpdateWalletCommandHandler(
    IWalletRepository walletRepository,
    IDateTimeService dateTimeService) : ICommandHandler<UpdateWalletCommand>
{
    public async Task<Result> Handle(
        UpdateWalletCommand request,
        CancellationToken cancellationToken)
    {
        // Get wallet
        var wallet = await walletRepository.GetByIdAsync(request.Id);
        if (wallet is null)
        {
            return Result.Fail(WalletErrors.NotFound);
        }

        // Update wallet
        var result = wallet.Update(
            request.Name,
            request.AllowNegative,
            Color.FromHex(request.Color),
            request.Notes,
            dateTimeService.UtcNow);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        walletRepository.Update(wallet);

        return Result.Ok();
    }
}