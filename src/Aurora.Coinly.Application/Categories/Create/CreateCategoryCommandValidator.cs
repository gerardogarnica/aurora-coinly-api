namespace Aurora.Coinly.Application.Categories.Create;

internal sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);

        RuleFor(x => x.MaxDaysToReverse)
            .NotEmpty()
            .InclusiveBetween(0, 15);

        RuleFor(x => x.Color)
            .NotEmpty()
            .Length(7);

        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}