namespace Aurora.Coinly.Domain.Summary;

public static class SummaryErrors
{
    public static readonly BaseError CurrencyNotMatched = BaseError.Validation(
        "Summary.CurrencyNotMatched",
        "The currency of the transaction does not match the currency of the summary");

    public static readonly BaseError NotFound = BaseError.NotFound(
        "Summary.NotFound",
        "The summary was not found");

    public static readonly BaseError TransactionAlreadyIsPaid = BaseError.Validation(
        "Summary.TransactionAlreadyIsPaid",
        "The transaction is already paid");

    public static readonly BaseError TransactionNotInPeriod = BaseError.Validation(
        "Summary.TransactionNotInPeriod",
        "The transaction is not in the summary period");

    public static readonly BaseError TransactionNotPaid = BaseError.Validation(
        "Summary.TransactionNotPaid",
        "The transaction is not paid");
}