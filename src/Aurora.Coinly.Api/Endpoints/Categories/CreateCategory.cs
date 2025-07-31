using Aurora.Coinly.Application.Categories.Create;
using Aurora.Coinly.Domain.Categories;
using Aurora.Coinly.Domain.Transactions;

namespace Aurora.Coinly.Api.Endpoints.Categories;

public sealed class CreateCategory : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "categories",
            async ([FromBody] CreateCategoryRequest request, IUserContext userContext, ISender sender) =>
            {
                var command = new CreateCategoryCommand(
                    userContext.UserId,
                    request.Name,
                    request.Group,
                    request.Type,
                    request.MaxDaysToReverse,
                    request.Color,
                    request.Notes);

                Result<Guid> result = await sender.Send(command);

                return result.Match(
                    () => Results.Created(string.Empty, result.Value),
                    ApiResponses.Problem);
            })
            .RequireAuthorization()
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
        int MaxDaysToReverse,
        string Color,
        [property: JsonConverter(typeof(JsonStringEnumConverter))]
        CategoryGroup Group,
        string? Notes);
}