namespace Aurora.Coinly.Application.Wallets.Transfer;

internal sealed class TransferBetweenWalletsCommandValidator : AbstractValidator<TransferBetweenWalletsCommand>
{
    public TransferBetweenWalletsCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .PrecisionScale(2, 9, true);
    }
}