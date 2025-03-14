using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Wallets.Create;

internal sealed class CreateWalletCommandHandler(
    IWalletRepository walletRepository,
    IDateTimeService dateTimeService) : IRequestHandler<CreateWalletCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateWalletCommand request,
        CancellationToken cancellationToken)
    {
        // Create wallet
        var wallet = Wallet.Create(
            request.Name,
            new Money(request.Amount, Currency.FromCode(request.CurrencyCode)),
            request.Type,
            request.Notes,
            dateTimeService.UtcNow);

        await walletRepository.AddAsync(wallet, cancellationToken);

        return wallet.Id;
    }
}