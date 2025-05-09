﻿using Aurora.Coinly.Application.Categories;
using Aurora.Coinly.Application.Categories.GetById;

namespace Aurora.Coinly.Api.Endpoints.Categories;

public sealed class GetCategoryById : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "categories/{id}",
            async (Guid id, ISender sender) =>
            {
                var query = new GetCategoryByIdQuery(id);

                Result<CategoryModel> result = await sender.Send(query);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .WithName("GetCategoryById")
            .WithTags(EndpointTags.Categories)
            .Produces<CategoryModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}