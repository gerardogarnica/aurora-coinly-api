using Aurora.Coinly.Application.Categories;
using Aurora.Coinly.Application.Categories.GetList;

namespace Aurora.Coinly.Api.Endpoints.Categories;

public sealed class GetCategories : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "categories",
            async ([FromQuery(Name = "deleted")] bool showDeleted, ISender sender) =>
            {
                var query = new GetCategoryListQuery(showDeleted);

                Result<IReadOnlyCollection<CategoryModel>> result = await sender.Send(query);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("GetCategories")
            .WithTags(EndpointTags.Categories)
            .Produces<IReadOnlyCollection<CategoryModel>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}