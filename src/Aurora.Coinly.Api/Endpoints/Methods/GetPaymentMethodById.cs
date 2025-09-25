using Aurora.Coinly.Application.Methods;
using Aurora.Coinly.Application.Methods.GetById;

namespace Aurora.Coinly.Api.Endpoints.Methods;

public sealed class GetPaymentMethodById : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "methods/{id}",
            async (
                Guid id,
                IQueryHandler<GetPaymentMethodByIdQuery, PaymentMethodModel> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new GetPaymentMethodByIdQuery(id);

                Result<PaymentMethodModel> result = await handler.Handle(query, cancellationToken);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("GetPaymentMethodById")
            .WithTags(EndpointTags.PaymentMethods)
            .Produces<PaymentMethodModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}