using Aurora.Coinly.Domain.Summary;
using Aurora.Coinly.Domain.Wallets;

namespace Aurora.Coinly.Application.Summary.UpdateSavings;

internal sealed class UpdateSummarySavingsCommandHandler(
    ICoinlyDbContext dbContext,
    IMonthlySummaryRepository summaryRepository) : ICommandHandler<UpdateSummarySavingsCommand>
{
    public async Task<Result> Handle(
        UpdateSummarySavingsCommand request,
        CancellationToken cancellationToken)
    {
        // Get wallet
        Wallet? wallet = await dbContext
            .Wallets
            .Include(x => x.Methods)
            .SingleOrDefaultAsync(x => x.Id == request.WalletId && x.UserId == request.UserId, cancellationToken);

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