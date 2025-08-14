using Aurora.Coinly.Domain.Summary;
using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Summary.UpdateSavings;

internal sealed class UpdateSummarySavingsCommandHandler(
    IMonthlySummaryRepository summaryRepository,
    IWalletRepository walletRepository) : ICommandHandler<UpdateSummarySavingsCommand>
{
    public async Task<Result> Handle(
        UpdateSummarySavingsCommand request,
        CancellationToken cancellationToken)
    {
        // Get wallet
        var wallet = await walletRepository.GetByIdAsync(request.WalletId, request.UserId);
        if (wallet is null)
        {
            return Result.Fail(WalletErrors.NotFound);
        }

        // Get monthly summary
        var monthlySummary = await summaryRepository.GetSummaryAsync(
            wallet.UserId,
            request.AssignedOn.Year,
            request.AssignedOn.Month,
            request.Amount.Currency.Code);

        var isNewSummary = monthlySummary is null;

        monthlySummary ??= MonthlySummary.Create(
            wallet.UserId,
            request.AssignedOn.Year,
            request.AssignedOn.Month,
            request.Amount.Currency);

        // Update savings
        var result = monthlySummary.UpdateSavings(request.Amount, request.AssignedOn, request.IsIncrement);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        if (isNewSummary)
        {
            await summaryRepository.AddAsync(result.Value, cancellationToken);
        }
        else
        {
            summaryRepository.Update(result.Value);
        }

        return Result.Ok();
    }
}