namespace Aurora.Coinly.Domain.Methods;

public static class PaymentMethodErrors
{
    public static readonly BaseError NotFound = new(
        "PaymentMethod.Found",
        "The payment method with the specified identifier was not found");

    public static readonly BaseError IsDeleted = new(
        "PaymentMethod.IsDeleted",
        "The payment method is already deleted");

    public static readonly BaseError IsUnavailableToReverse = new(
        "PaymentMethod.IsUnavailableToReverse",
        "The payment method is unavailable to reverse the transaction");
}