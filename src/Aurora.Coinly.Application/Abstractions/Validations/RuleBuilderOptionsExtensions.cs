namespace Aurora.Coinly.Application.Abstractions.Validations;

public static class RuleBuilderOptionsExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithBaseError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        BaseError baseError) =>
        rule.WithErrorCode(baseError.Code).WithMessage(baseError.Message);

    public static IRuleBuilderOptions<T, string> MustBeValidColor<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty()
            .MaximumLength(Color.MaxLength);
}