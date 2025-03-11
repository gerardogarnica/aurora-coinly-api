namespace Aurora.Coinly.Application.Wallets.GetHistory;

internal sealed class GetWalletHistoryQueryValidator : AbstractValidator<GetWalletHistoryQuery>
{
    public GetWalletHistoryQueryValidator()
    {
        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom);
    }
}