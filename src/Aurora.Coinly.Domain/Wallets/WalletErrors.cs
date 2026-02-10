namespace Aurora.Coinly.Domain.Wallets;

public static class WalletErrors
{
    public static readonly BaseError NotFound = BaseError.NotFound(
        "Wallet.NotFound",
        "The wallet with the specified identifier was not found");

    public static readonly BaseError IsDeleted = BaseError.Validation(
        "Wallet.IsDeleted",
        "The wallet is already deleted");

    public static readonly BaseError CurrenciesNotMatch = BaseError.Validation(
        "Wallet.CurrenciesNotMatch",
        "The currencies of the wallets do not match");

    public static readonly BaseError HasActivePaymentMethods = BaseError.Validation(
        "Wallet.HasActivePaymentMethods",
        "The wallet has active payment methods");

    public static readonly BaseError InsufficientFunds = BaseError.Validation(
        "Wallet.InsufficientFunds",
        "The wallet does not have sufficient funds to perform this operation");

    public static readonly BaseError TransactionNotBelongs = BaseError.Validation(
        "Wallet.TransactionNotBelongs",
        "The transaction does not belong to the wallet");

    public static readonly BaseError UnableToAssignToAvailable = BaseError.Validation(
        "Wallet.UnableToAssignToAvailable",
        "Insufficient funds to assign to available amount");

    public static readonly BaseError UnableToAssignToSavings = BaseError.Validation(
        "Wallet.UnableToAssignToSavings",
        "Insufficient funds to assign to savings");
}