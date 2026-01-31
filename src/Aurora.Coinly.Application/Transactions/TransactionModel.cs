namespace Aurora.Coinly.Application.Transactions;

public sealed record TransactionModel(
    Guid TransactionId,
    string Description,
    DateOnly TransactionDate,
    DateOnly MaxPaymentDate,
    DateOnly? PaymentDate,
    DateOnly DisplayDate,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    DisplayDateType DisplayDateType,
    string Currency,
    decimal Amount,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    TransactionType Type,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    TransactionStatus Status,
    bool IsPaid,
    TransactionCategoryModel Category,
    TransactionPaymentMethodModel? PaymentMethod,
    TransactionWalletModel? Wallet,
    string? Notes,
    bool IsRecurring,
    int InstallmentNumber);

public sealed record TransactionCategoryModel(
    Guid CategoryId,
    string Name,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    TransactionType Type,
    string Color,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    CategoryGroup Group);

public sealed record TransactionPaymentMethodModel(
    Guid PaymentMethodId,
    string Name);

public sealed record TransactionWalletModel(
    Guid WalletId,
    string Name,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    WalletType Type,
    string Color);