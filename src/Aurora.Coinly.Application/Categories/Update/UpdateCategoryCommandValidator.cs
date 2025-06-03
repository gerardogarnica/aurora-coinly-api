namespace Aurora.Coinly.Application.Categories.Update;

internal sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
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