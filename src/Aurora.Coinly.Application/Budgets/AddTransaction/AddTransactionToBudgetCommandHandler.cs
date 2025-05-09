﻿using Aurora.Coinly.Domain.Budgets;
using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Application.Budgets.AddTransaction;

internal sealed class AddTransactionToBudgetCommandHandler(
    IBudgetRepository budgetRepository,
    ITransactionRepository transactionRepository) : ICommandHandler<AddTransactionToBudgetCommand>
{
    public async Task<Result> Handle(
        AddTransactionToBudgetCommand request,
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

        // Get budget
        var budget = await budgetRepository.GetByCategoryIdAsync(
            transaction.CategoryId,
            transaction.PaymentDate!.Value);
        if (budget is null)
        {
            // Because budget is not neccesary for transaction
            return Result.Ok();
        }

        // Add transaction to budget
        var result = budget.AssignTransaction(transaction);

        if (!result.IsSuccessful)
        {
            return Result.Fail(result.Error);
        }

        budgetRepository.Update(budget);

        return Result.Ok();
    }
}