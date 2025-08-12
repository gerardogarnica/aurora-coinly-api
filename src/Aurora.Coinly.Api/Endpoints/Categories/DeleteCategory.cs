using Aurora.Coinly.Application.Categories.Delete;

namespace Aurora.Coinly.Api.Endpoints.Categories;

public sealed class DeleteCategory : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
            "categories/{id}",
            async (Guid id, ISender sender) =>
            {
                var command = new DeleteCategoryCommand(id);

                Result result = await sender.Send(command);

                return result.Match(
                    () => Results.Accepted(string.Empty),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("DeleteCategory")
            .WithTags(EndpointTags.Categories)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}