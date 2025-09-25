using Aurora.Coinly.Application.Transactions.Remove;

namespace Aurora.Coinly.Api.Endpoints.Transactions;

public sealed class RemoveTransaction : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
            "transactions/{id}",
            async (
                Guid id,
                ICommandHandler<RemoveTransactionCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new RemoveTransactionCommand(id);

                Result result = await handler.Handle(command, cancellationToken);

                return result.Match(
                    () => Results.Accepted(string.Empty),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("RemoveTransaction")
            .WithTags(EndpointTags.Transactions)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}