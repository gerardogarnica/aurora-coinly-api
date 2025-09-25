using Aurora.Coinly.Application.Categories;
using Aurora.Coinly.Application.Categories.GetById;

namespace Aurora.Coinly.Api.Endpoints.Categories;

public sealed class GetCategoryById : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "categories/{id}",
            async (
                Guid id,
                IQueryHandler<GetCategoryByIdQuery, CategoryModel> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new GetCategoryByIdQuery(id);

                Result<CategoryModel> result = await handler.Handle(query, cancellationToken);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("GetCategoryById")
            .WithTags(EndpointTags.Categories)
            .Produces<CategoryModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}