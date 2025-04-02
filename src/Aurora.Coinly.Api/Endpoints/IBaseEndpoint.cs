namespace Aurora.Coinly.Api.Endpoints;

public interface IBaseEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}