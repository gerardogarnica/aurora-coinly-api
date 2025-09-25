using Aurora.Coinly.Application.Budgets.UpdateLimit;

namespace Aurora.Coinly.Api.Endpoints.Budgets;

public sealed class UpdateBudget : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "budgets/{id}",
            async (
                Guid id,
                [FromBody] UpdateBudgetRequest request,
                ICommandHandler<UpdateBudgetLimitCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateBudgetLimitCommand(id, request.PeriodId, request.NewAmountLimit);

                Result result = await handler.Handle(command, cancellationToken);

                return result.Match(
                    () => Results.Accepted(string.Empty),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("UpdateBudget")
            .WithTags(EndpointTags.Budgets)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record UpdateBudgetRequest(Guid PeriodId, decimal NewAmountLimit);
}