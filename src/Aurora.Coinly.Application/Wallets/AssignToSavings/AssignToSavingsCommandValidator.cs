namespace Aurora.Coinly.Application.Wallets.AssignToSavings;

internal sealed class AssignToSavingsCommandValidator : AbstractValidator<AssignToSavingsCommand>
{
    public AssignToSavingsCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .PrecisionScale(2, 9, true);
    }
}