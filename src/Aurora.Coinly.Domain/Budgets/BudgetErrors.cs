namespace Aurora.Coinly.Domain.Budgets;

public static class BudgetErrors
{
    public static readonly BaseError NotFound = BaseError.NotFound(
        "Budget.NotFound",
        "The budget with the specified identifier was not found");

    public static readonly BaseError BudgetAlreadyExists = BaseError.Conflict(
        "Budget.BudgetAlreadyExists",
        "The budget already exists for the specified category and year");

    public static readonly BaseError IsClosed = BaseError.Validation(
        "Budget.IsClosed",
        "The budget is closed");

    public static readonly BaseError PeriodNotFound = BaseError.NotFound(
        "Budget.PeriodNotFound",
        "The budget period with the specified identifier was not found");

    public static readonly BaseError TransactionAlreadyIsPaid = BaseError.Validation(
        "Budget.TransactionAlreadyIsPaid",
        "The transaction has already been paid");

    public static readonly BaseError TransactionCategoryMismatch = BaseError.Conflict(
        "Budget.TransactionCategoryMismatch",
        "The transaction category does not match the budget category");

    public static readonly BaseError TransactionNotBelongs = BaseError.Conflict(
        "Budget.TransactionNotBelongs",
        "The transaction does not belong to the budget");

    public static readonly BaseError TransactionNotPaid = BaseError.Validation(
        "Budget.TransactionNotPaid",
        "The transaction is not paid");

    public static readonly BaseError TransactionPaymentDateOutOfRange = BaseError.Validation(
        "Budget.TransactionPaymentDateOutOfRange",
        "The transaction payment date is out of the budget period range");
}