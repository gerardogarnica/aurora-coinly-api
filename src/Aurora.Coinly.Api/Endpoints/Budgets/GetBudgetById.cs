using Aurora.Coinly.Application.Budgets;
using Aurora.Coinly.Application.Budgets.GetById;

namespace Aurora.Coinly.Api.Endpoints.Budgets;

public sealed class GetBudgetById : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "budgets/{id}",
            async (
                Guid id,
                IQueryHandler<GetBudgetByIdQuery, BudgetModel> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new GetBudgetByIdQuery(id);

                Result<BudgetModel> result = await handler.Handle(query, cancellationToken);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("GetBudgetById")
            .WithTags(EndpointTags.Budgets)
            .Produces<BudgetModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}