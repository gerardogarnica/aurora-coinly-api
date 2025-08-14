using Aurora.Coinly.Application.Transactions.CreateExpense;

namespace Aurora.Coinly.Api.Endpoints.Transactions;

public sealed class CreateExpenseTransaction : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "transactions/expense",
            async ([FromBody] CreateExpenseTransactionRequest request, ISender sender) =>
            {
                var command = new CreateExpenseTransactionCommand(
                    request.CategoryId,
                    request.PaymentMethodId,
                    request.Description,
                    request.TransactionDate,
                    request.CurrencyCode,
                    request.Amount,
                    request.Notes,
                    request.MakePayment,
                    request.WalletId);

                Result<Guid> result = await sender.Send(command);

                return result.Match(
                    () => Results.Created(string.Empty, result.Value),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("CreateExpenseTransaction")
            .WithTags(EndpointTags.Transactions)
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record CreateExpenseTransactionRequest(
        Guid CategoryId,
        Guid PaymentMethodId,
        string Description,
        DateOnly TransactionDate,
        string CurrencyCode,
        decimal Amount,
        string? Notes,
        bool MakePayment,
        Guid? WalletId);
}