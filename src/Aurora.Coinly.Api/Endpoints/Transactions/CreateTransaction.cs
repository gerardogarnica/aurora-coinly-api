using Aurora.Coinly.Application.Transactions.Create;

namespace Aurora.Coinly.Api.Endpoints.Transactions;

public sealed class CreateTransaction : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "transactions",
            async ([FromBody] CreateTransactionRequest request, ISender sender) =>
            {
                var command = new CreateTransactionCommand(
                    request.Description,
                    request.CategoryId,
                    request.PaymentMethodId,
                    request.TransactionDate,
                    request.MaxPaymentDate,
                    request.CurrencyCode,
                    request.Amount,
                    request.Notes,
                    request.Installment,
                    request.WalletId);

                Result<Guid> result = await sender.Send(command);

                return result.Match(
                    () => Results.Created(string.Empty, result.Value),
                    ApiResponses.Problem);
            })
            .WithName("CreateTransaction")
            .WithTags(EndpointTags.Transactions)
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record CreateTransactionRequest(
        string Description,
        Guid CategoryId,
        Guid PaymentMethodId,
        DateOnly TransactionDate,
        DateOnly MaxPaymentDate,
        string CurrencyCode,
        decimal Amount,
        string? Notes,
        int Installment,
        Guid? WalletId);
}