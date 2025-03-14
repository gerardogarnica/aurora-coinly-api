using Aurora.Coinly.Domain.Summary;
using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Summary.AddTransaction;

internal sealed class AddTransactionToSummaryCommandHandler(
    IMonthlySummaryRepository summaryRepository,
    ITransactionRepository transactionRepository) : ICommandHandler<AddTransactionToSummaryCommand>
{
    public async Task<Result> Handle(
        AddTransactionToSummaryCommand request,
        CancellationToken cancellationToken)
    {
        // Get transaction
        var transaction = await transactionRepository.GetByIdAsync(request.TransactionId);
        if (transaction is null)
        {
            return Result.Fail(TransactionErrors.NotFound);
        }

        if (!transaction.IsPaid)
        {
            return Result.Fail(TransactionErrors.NotPaid);
        }

        // Get monthly summary
        var monthlySummary = await summaryRepository.GetSummaryAsync(
            transaction.PaymentDate!.Value.Year,
            transaction.PaymentDate!.Value.Month,
            transaction.Amount.Currency.Code);

        var isNewSummary = monthlySummary is null;

        var result = isNewSummary
            ? CreateSummary(transaction)
            : UpdateSummary(monthlySummary!, transaction);

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

    private Result<MonthlySummary> CreateSummary(Transaction transaction)
    {
        var summary = MonthlySummary.Create(
            transaction.PaymentDate!.Value.Year,
            transaction.PaymentDate!.Value.Month,
            transaction.Amount.Currency);

        return summary.ApplyTransaction(transaction);
    }

    private Result<MonthlySummary> UpdateSummary(MonthlySummary summary, Transaction transaction)
    {
        return summary.ApplyTransaction(transaction);
    }
}