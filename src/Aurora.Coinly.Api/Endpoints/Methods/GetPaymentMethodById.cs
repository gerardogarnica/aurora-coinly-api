using Aurora.Coinly.Application.Methods;
using Aurora.Coinly.Application.Methods.GetById;

namespace Aurora.Coinly.Api.Endpoints.Methods;

public sealed class GetPaymentMethodById : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "methods/{id}",
            async (Guid id, ISender sender) =>
            {
                var query = new GetPaymentMethodByIdQuery(id);

                Result<PaymentMethodModel> result = await sender.Send(query);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .WithName("GetPaymentMethodById")
            .WithTags(EndpointTags.PaymentMethods)
            .Produces<PaymentMethodModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}