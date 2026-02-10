namespace Aurora.Coinly.Domain.Transactions;

public static class TransactionErrors
{
    public static readonly BaseError NotFound = BaseError.NotFound(
        "Transaction.Found",
        "The transaction with the specified identifier was not found");

    public static readonly BaseError AlreadyPaid = BaseError.Validation(
        "Transaction.AlreadyPaid",
        "The transaction has already been paid");

    public static readonly BaseError AlreadyRemoved = BaseError.Validation(
        "Transaction.AlreadyRemoved",
        "The transaction has already been removed");

    public static readonly BaseError InvalidMaxPaymentDate = BaseError.Validation(
        "Transaction.InvalidMaxPaymentDate",
        "The maximum payment date must be equal to the transaction date");

    public static BaseError InvalidPaymentDate(DateOnly transactionDate) => BaseError.Validation(
        "Transaction.InvalidPaymentDate",
        $"Payment date must be greater than or equal to the '{transactionDate}'");

    public static readonly BaseError NotPaid = BaseError.Validation(
        "Transaction.NotPaid",
        "The transaction is not in paid status");

    public static readonly BaseError NotPending = BaseError.Validation(
        "Transaction.NotPending",
        "The transaction is not in pending status");
}