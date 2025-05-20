namespace Aurora.Coinly.Application.Wallets.Update;

internal sealed class UpdateWalletCommandValidator : AbstractValidator<UpdateWalletCommand>
{
    public UpdateWalletCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);

        RuleFor(x => x.Color)
            .NotEmpty()
            .Length(7);

        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}