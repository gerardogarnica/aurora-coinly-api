using Aurora.Coinly.Application.Budgets;
using Aurora.Coinly.Application.Budgets.GetList;

namespace Aurora.Coinly.Api.Endpoints.Budgets;

public sealed class GetBudgets : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "budgets/year/{year}",
            async (int year, ISender sender) =>
            {
                var query = new GetBudgetListQuery(year);

                Result<IReadOnlyCollection<BudgetModel>> result = await sender.Send(query);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("GetBudgets")
            .WithTags(EndpointTags.Budgets)
            .Produces<IReadOnlyCollection<BudgetModel>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}