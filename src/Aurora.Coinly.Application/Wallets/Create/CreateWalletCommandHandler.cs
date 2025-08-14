using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Wallets.Create;

internal sealed class CreateWalletCommandHandler(
    IWalletRepository walletRepository,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<CreateWalletCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateWalletCommand request,
        CancellationToken cancellationToken)
    {
        // Create wallet
        var wallet = Wallet.Create(
            userContext.UserId,
            request.Name,
            new Money(request.Amount, Currency.FromCode(request.CurrencyCode)),
            request.Type,
            request.AllowNegative,
            Color.FromHex(request.Color),
            request.Notes,
            request.OpenedOn,
            dateTimeService.UtcNow);

        await walletRepository.AddAsync(wallet, cancellationToken);

        return wallet.Id;
    }
}