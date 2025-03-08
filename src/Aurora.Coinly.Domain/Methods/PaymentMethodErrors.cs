namespace Aurora.Coinly.Domain.Methods;

public static class PaymentMethodErrors
{
    public static readonly BaseError NotFound = new(
        "PaymentMethod.Found",
        "The payment method with the specified identifier was not found");

    public static readonly BaseError IsDeleted = new(
        "PaymentMethod.IsDeleted",
        "The payment method is deleted");
}