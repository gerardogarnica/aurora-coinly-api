namespace Aurora.Coinly.Application.Authentication.ChangePassword;

internal sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .MaximumLength(PasswordRuleExtensions.MaxPasswordLength);

        RuleFor(x => x.NewPassword)
            .MustBeStrongPassword()
            .NotEqual(x => x.CurrentPassword).WithBaseError(AuthenticationErrors.NewPasswordMustDiffer);
    }
}
