using Aurora.Coinly.Application.Methods.Delete;

namespace Aurora.Coinly.Api.Endpoints.Methods;

public sealed class DeletePaymentMethod : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
            "methods/{id}",
            async (
                Guid id,
                ICommandHandler<DeletePaymentMethodCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new DeletePaymentMethodCommand(id);

                Result result = await handler.Handle(command, cancellationToken);

                return result.Match(
                    () => Results.Accepted(string.Empty),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("DeletePaymentMethod")
            .WithTags(EndpointTags.PaymentMethods)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}