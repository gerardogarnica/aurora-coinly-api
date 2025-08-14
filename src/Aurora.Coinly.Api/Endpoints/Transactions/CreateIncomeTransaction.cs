using Aurora.Coinly.Application.Transactions.CreateIncome;

namespace Aurora.Coinly.Api.Endpoints.Transactions;

public sealed class CreateIncomeTransaction : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "transactions/income",
            async ([FromBody] CreateIncomeTransactionRequest request, ISender sender) =>
            {
                var command = new CreateIncomeTransactionCommand(
                    request.CategoryId,
                    request.Description,
                    request.TransactionDate,
                    request.CurrencyCode,
                    request.Amount,
                    request.Notes,
                    request.WalletId);

                Result<Guid> result = await sender.Send(command);

                return result.Match(
                    () => Results.Created(string.Empty, result.Value),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("CreateIncomeTransaction")
            .WithTags(EndpointTags.Transactions)
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record CreateIncomeTransactionRequest(
        Guid CategoryId,
        string Description,
        DateOnly TransactionDate,
        string CurrencyCode,
        decimal Amount,
        string? Notes,
        Guid WalletId);
}