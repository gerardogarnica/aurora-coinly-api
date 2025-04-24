namespace Aurora.Coinly.Domain.Transactions;

public static class TransactionErrors
{
    public static readonly BaseError NotFound = new(
        "Transaction.Found",
        "The transaction with the specified identifier was not found");

    public static readonly BaseError AlreadyPaid = new(
        "Transaction.AlreadyPaid",
        "The transaction has already been paid");

    public static readonly BaseError AlreadyRemoved = new(
        "Transaction.AlreadyRemoved",
        "The transaction has already been removed");

    public static readonly BaseError InvalidMaxPaymentDate = new(
        "Transaction.InvalidMaxPaymentDate",
        "The maximum payment date must be equal to the transaction date");

    public static BaseError InvalidPaymentDate(DateOnly transactionDate) => new(
        "Transaction.InvalidPaymentDate",
        $"Payment date must be greater than or equal to the '{transactionDate}'");

    public static readonly BaseError NotPaid = new(
        "Transaction.NotPaid",
        "The transaction is not in paid status");

    public static readonly BaseError NotPending = new(
        "Transaction.NotPending",
        "The transaction is not in pending status");
}