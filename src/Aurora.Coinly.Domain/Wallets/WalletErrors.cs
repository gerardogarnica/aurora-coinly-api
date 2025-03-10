namespace Aurora.Coinly.Domain.Wallets;

public static class WalletErrors
{
    public static readonly BaseError NotFound = new(
        "Wallet.NotFound",
        "The wallet with the specified identifier was not found");

    public static readonly BaseError IsDeleted = new(
        "Wallet.IsDeleted",
        "The wallet is deleted");

    public static readonly BaseError CurrenciesNotMatch = new(
        "Wallet.CurrenciesNotMatch",
        "The currencies of the wallets do not match");

    public static readonly BaseError UnableToAssignToAvailable = new(
        "Wallet.UnableToAssignToAvailable",
        "Insufficient funds to assign to available amount");

    public static readonly BaseError UnableToAssignToSavings = new(
        "Wallet.UnableToAssignToSavings",
        "Insufficient funds to assign to savings");
}