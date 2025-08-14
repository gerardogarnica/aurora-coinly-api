using Aurora.Coinly.Application.Methods.SetDefault;

namespace Aurora.Coinly.Api.Endpoints.Methods;

public sealed class SetDefaultPaymentMethod : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "methods/{id}/default",
            async (Guid id, ISender sender) =>
            {
                var command = new SetDefaultPaymentMethodCommand(id);

                Result result = await sender.Send(command);

                return result.Match(
                    () => Results.Accepted(string.Empty),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("SetDefaultPaymentMethod")
            .WithTags(EndpointTags.PaymentMethods)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}