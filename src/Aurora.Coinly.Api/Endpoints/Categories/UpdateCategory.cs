using Aurora.Coinly.Application.Categories.Update;
using Aurora.Coinly.Domain.Categories;

namespace Aurora.Coinly.Api.Endpoints.Categories;

public sealed class UpdateCategory : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "categories/{id}",
            async (
                Guid id,
                [FromBody] UpdateCategoryRequest request,
                ICommandHandler<UpdateCategoryCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateCategoryCommand(
                    id,
                    request.Name,
                    request.MaxDaysToReverse,
                    request.Color,
                    request.Group,
                    request.Notes);

                Result result = await handler.Handle(command, cancellationToken);

                return result.Match(
                    () => Results.Accepted(string.Empty),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
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
        [property: JsonConverter(typeof(JsonStringEnumConverter))]
        CategoryGroup Group,
        string? Notes);
}