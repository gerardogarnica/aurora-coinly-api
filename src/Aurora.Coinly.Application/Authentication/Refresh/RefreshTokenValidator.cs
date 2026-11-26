namespace Aurora.Coinly.Application.Authentication.Refresh;

internal sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .MaximumLength(UserToken.MaxRefreshTokenLength);
    }
}
