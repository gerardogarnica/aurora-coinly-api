using Aurora.Coinly.Application.Methods.Update;

namespace Aurora.Coinly.Api.Endpoints.Methods;

public sealed class UpdatePaymentMethod : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "methods/{id}",
            async (Guid id, [FromBody] UpdatePaymentMethodRequest request, ISender sender) =>
            {
                var command = new UpdatePaymentMethodCommand(
                    id,
                    request.Name,
                    request.AllowRecurring,
                    request.AutoMarkAsPaid,
                    request.RelatedWalletId,
                    request.SuggestedPaymentDay,
                    request.StatementCutoffDay,
                    request.Notes);

                Result result = await sender.Send(command);

                return result.Match(
                    () => Results.Accepted(string.Empty),
                    ApiResponses.Problem);
            })
            .WithName("UpdatePaymentMethod")
            .WithTags(EndpointTags.PaymentMethods)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record UpdatePaymentMethodRequest(
        string Name,
        bool AllowRecurring,
        bool AutoMarkAsPaid,
        Guid RelatedWalletId,
        int? SuggestedPaymentDay,
        int? StatementCutoffDay,
        string? Notes);
}