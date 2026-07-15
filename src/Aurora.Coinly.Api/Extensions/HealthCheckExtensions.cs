namespace Aurora.Coinly.Api.Extensions;

internal static class HealthCheckExtensions
{
    internal static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health");

        return app;
    }
}
