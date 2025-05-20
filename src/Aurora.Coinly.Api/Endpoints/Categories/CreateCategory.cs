using Aurora.Coinly.Application.Categories.Create;
using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Api.Endpoints.Categories;

public sealed class CreateCategory : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "categories",
            async ([FromBody] CreateCategoryRequest request, ISender sender) =>
            {
                var command = new CreateCategoryCommand(
                    request.Name,
                    request.Type,
                    request.Color,
                    request.Notes);

                Result<Guid> result = await sender.Send(command);

                return result.Match(
                    () => Results.Created(string.Empty, result.Value),
                    ApiResponses.Problem);
            })
            .WithName("CreateCategory")
            .WithTags(EndpointTags.Categories)
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record CreateCategoryRequest(
        string Name,
        [property: JsonConverter(typeof(JsonStringEnumConverter))]
        TransactionType Type,
        string Color,
        string? Notes);
}