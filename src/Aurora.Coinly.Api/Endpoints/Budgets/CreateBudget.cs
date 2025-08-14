using Aurora.Coinly.Application.Budgets.Create;
using Aurora.Coinly.Domain.Budgets;

namespace Aurora.Coinly.Api.Endpoints.Budgets;

public sealed class CreateBudget : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "budgets",
            async ([FromBody] CreateBudgetRequest request, ISender sender) =>
            {
                var command = new CreateBudgetCommand(
                    request.CategoryId,
                    request.Year,
                    request.Frequency,
                    request.CurrencyCode,
                    request.AmountLimit);

                Result<Guid> result = await sender.Send(command);

                return result.Match(
                    () => Results.Created(string.Empty, result.Value),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("CreateBudget")
            .WithTags(EndpointTags.Budgets)
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record CreateBudgetRequest(
        Guid CategoryId,
        int Year,
        [property: JsonConverter(typeof(JsonStringEnumConverter))]
        BudgetFrequency Frequency,
        string CurrencyCode,
        decimal AmountLimit);
}