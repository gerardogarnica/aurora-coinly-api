using FluentValidation;

namespace Aurora.Coinly.Application.Transactions.Process;

internal sealed class ProcessTransactionPaymentCommandValidator : AbstractValidator<ProcessTransactionPaymentCommand>
{
    public ProcessTransactionPaymentCommandValidator(IDateTimeService dateTimeService)
    {
        RuleFor(x => x.PaymentDate)
            .NotEmpty()
            .LessThanOrEqualTo(dateTimeService.Today)
            .WithMessage("'{PropertyName}' must be today or in the past.");
    }
}