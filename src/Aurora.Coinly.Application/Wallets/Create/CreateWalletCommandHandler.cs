namespace Aurora.Coinly.Application.Wallets.Create;

internal sealed class CreateWalletCommandHandler(
    ICoinlyDbContext dbContext,
    IUserContext userContext,
    IDateTimeService dateTimeService) : ICommandHandler<CreateWalletCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateWalletCommand request,
        CancellationToken cancellationToken)
    {
        Result<Color> color = Color.Create(request.Color);
        if (!color.IsSuccessful)
        {
            return Result.Fail<Guid>(color.Error);
        }

        // Create wallet
        var wallet = Wallet.Create(
            userContext.UserId,
            request.Name,
            new Money(request.Amount, Currency.FromCode(request.CurrencyCode)),
            request.Type,
            request.AllowNegative,
            color.Value,
            request.Notes,
            request.OpenedOn,
            dateTimeService.UtcNow);

        dbContext.Wallets.Add(wallet);

        await dbContext.SaveChangesAsync(cancellationToken);

        return wallet.Id;
    }
}