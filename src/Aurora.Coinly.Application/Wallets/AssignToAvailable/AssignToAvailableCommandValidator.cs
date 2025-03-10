namespace Aurora.Coinly.Application.Wallets.AssignToAvailable;

internal sealed class AssignToAvailableCommandValidator : AbstractValidator<AssignToAvailableCommand>
{
    public AssignToAvailableCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .PrecisionScale(2, 9, true);
    }
}