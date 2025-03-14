namespace Aurora.Coinly.Domain.Summary;

public static class SummaryErrors
{
    public static readonly BaseError CurrencyNotMatched = new(
        "Summary.CurrencyNotMatched",
        "The currency of the transaction does not match the currency of the summary");

    public static readonly BaseError TransactionAlreadyIsPaid = new(
        "Summary.TransactionAlreadyIsPaid",
        "The transaction is already paid");

    public static readonly BaseError TransactionNotInPeriod = new(
        "Summary.TransactionNotInPeriod",
        "The transaction is not in the summary period");

    public static readonly BaseError TransactionNotPaid = new(
        "Summary.TransactionNotPaid",
        "The transaction is not paid");
}