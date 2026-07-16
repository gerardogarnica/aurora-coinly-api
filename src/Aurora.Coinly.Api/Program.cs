using Aurora.Coinly.Api;
using Aurora.Coinly.Api.Endpoints;
using Aurora.Coinly.Api.Extensions;
using Aurora.Coinly.Application;
using Aurora.Coinly.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder
    .AddApiServices()
    .AddErrorHandling()
    .AddObservability()
    .AddHealthCheckServices()
    .AddCorsServices();

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddEndpoints();

var app = builder.Build();

RouteGroupBuilder routeGroup = app
    .MapGroup("aurora/coinly/");

app.MapEndpoints(routeGroup);
app.MapHealthCheckEndpoints();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "Coinly API";
});

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    await app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();

app.UseCors(CorsExtensions.FrontendPolicy);

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseAuthentication();
app.UseAuthorization();

await app.RunAsync();

// REMARK: Required for functional and integration tests to work.
namespace Aurora.Coinly.Api
{
    public partial class Program;
}