using Aurora.Coinly.Api.Endpoints;
using Aurora.Coinly.Api.Extensions;
using Aurora.Coinly.Api.Middlewares;
using Aurora.Coinly.Application;
using Aurora.Coinly.Infrastructure;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Coinly API", Version = "v1" });
});
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(cfg => cfg.AddService(builder.Environment.ApplicationName))
    .WithTracing(cfg => cfg
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation())
    .WithMetrics(cfg => cfg
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation())
    .UseOtlpExporter();

builder.Logging.AddOpenTelemetry(cfg =>
{
    cfg.IncludeScopes = true;
    cfg.IncludeFormattedMessage = true;
});

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddEndpoints();

var app = builder.Build();

RouteGroupBuilder routeGroup = app
    .MapGroup("aurora/coinly/")
    .WithOpenApi();

app.MapEndpoints(routeGroup);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "Coinly API";
    });

    app.ApplyMigrations();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

await app.RunAsync();
