using Aurora.Coinly.Application.Methods;
using Aurora.Coinly.Application.Methods.GetList;

namespace Aurora.Coinly.Api.Endpoints.Methods;

public sealed class GetPaymentMethods : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "methods",
            async (
                [FromQuery(Name = "deleted")] bool showDeleted,
                IQueryHandler<GetPaymentMethodListQuery, IReadOnlyCollection<PaymentMethodModel>> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new GetPaymentMethodListQuery(showDeleted);

                Result<IReadOnlyCollection<PaymentMethodModel>> result = await handler.Handle(query, cancellationToken);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("GetPaymentMethods")
            .WithTags(EndpointTags.PaymentMethods)
            .Produces<IReadOnlyCollection<PaymentMethodModel>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}