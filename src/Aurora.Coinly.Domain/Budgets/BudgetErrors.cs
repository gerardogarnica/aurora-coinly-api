namespace Aurora.Coinly.Domain.Budgets;

public static class BudgetErrors
{
    public static readonly BaseError NotFound = new(
        "Budget.NotFound",
        "The budget with the specified identifier was not found");

    public static readonly BaseError BudgetAlreadyExists = new(
        "Budget.BudgetAlreadyExists",
        "The budget already exists for the specified category and year");

    public static readonly BaseError IsClosed = new(
        "Budget.IsClosed",
        "The budget is closed");

    public static readonly BaseError PeriodNotFound = new(
        "Budget.PeriodNotFound",
        "The budget period with the specified identifier was not found");

    public static readonly BaseError TransactionAlreadyIsPaid = new(
        "Budget.TransactionAlreadyIsPaid",
        "The transaction has already been paid");

    public static readonly BaseError TransactionCategoryMismatch = new(
        "Budget.TransactionCategoryMismatch",
        "The transaction category does not match the budget category");

    public static readonly BaseError TransactionNotBelongs = new(
        "Budget.TransactionNotBelongs",
        "The transaction does not belong to the budget");

    public static readonly BaseError TransactionNotPaid = new(
        "Budget.TransactionNotPaid",
        "The transaction is not paid");

    public static readonly BaseError TransactionPaymentDateOutOfRange = new(
        "Budget.TransactionPaymentDateOutOfRange",
        "The transaction payment date is out of the budget period range");
}