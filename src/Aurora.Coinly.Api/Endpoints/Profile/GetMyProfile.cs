using Aurora.Coinly.Application.Users;
using Aurora.Coinly.Application.Users.GetById;

namespace Aurora.Coinly.Api.Endpoints.Profile;

public sealed class GetMyProfile : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "me",
            async (IUserContext userContext, ISender sender) =>
            {
                var query = new GetUserByIdQuery(userContext.UserId);

                Result<UserModel> result = await sender.Send(query);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("GetMyProfile")
            .WithTags(EndpointTags.Profile)
            .Produces<UserModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}