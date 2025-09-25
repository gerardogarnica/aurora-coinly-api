using Aurora.Coinly.Application.Dashboard;
using Aurora.Coinly.Application.Dashboard.GetSummary;

namespace Aurora.Coinly.Api.Endpoints.Dashboard;

public sealed class GetDashboard : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "dashboard",
            async (
                IQueryHandler<GetDashboardSummaryQuery, DashboardSummaryModel> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new GetDashboardSummaryQuery();

                Result<DashboardSummaryModel> result = await handler.Handle(query, cancellationToken);

                return result.Match(Results.Ok, ApiResponses.Problem);
            })
            .RequireAuthorization()
            .WithName("GetDashboard")
            .WithTags(EndpointTags.Dashboard)
            .Produces<DashboardSummaryModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}