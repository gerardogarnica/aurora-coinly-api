using Aurora.Coinly.Application.Categories.Update;

namespace Aurora.Coinly.Api.Endpoints.Categories;

public sealed class UpdateCategory : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "categories/{id}",
            async (Guid id, [FromBody] UpdateCategoryRequest request, ISender sender) =>
            {
                var command = new UpdateCategoryCommand(
                    id,
                    request.Name,
                    request.MaxDaysToReverse,
                    request.Color,
                    request.Notes);

                Result result = await sender.Send(command);

                return result.Match(
                    () => Results.Accepted(string.Empty),
                    ApiResponses.Problem);
            })
            .WithName("UpdateCategory")
            .WithTags(EndpointTags.Categories)
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record UpdateCategoryRequest(
        string Name,
        int MaxDaysToReverse,
        string Color,
        string? Notes);
}