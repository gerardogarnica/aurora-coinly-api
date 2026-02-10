namespace Aurora.Coinly.Domain.Methods;

public static class PaymentMethodErrors
{
    public static readonly BaseError NotFound = BaseError.NotFound(
        "PaymentMethod.Found",
        "The payment method with the specified identifier was not found");

    public static readonly BaseError IsDeleted = BaseError.Validation(
        "PaymentMethod.IsDeleted",
        "The payment method is already deleted");

    public static readonly BaseError IsUnavailableToReverse = BaseError.Validation(
        "PaymentMethod.IsUnavailableToReverse",
        "The payment method is unavailable to reverse the transaction");
}