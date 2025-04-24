namespace Aurora.Coinly.Application.Wallets.AssignToAvailable;

internal sealed class AssignToAvailableCommandValidator : AbstractValidator<AssignToAvailableCommand>
{
    public AssignToAvailableCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .PrecisionScale(9, 2, true);
    }
}