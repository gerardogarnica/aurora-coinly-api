using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Aurora.Coinly.Application.Abstractions.Behaviors;

internal sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing request: {Name} {@Request}", typeof(TRequest).Name, request);

        TResponse result = await next();

        if (result.IsSuccessful)
        {
            logger.LogInformation("Request processed successfully: {Name} {@Response}", typeof(TResponse).Name, result);
        }
        else
        {
            using (LogContext.PushProperty("Errors", result.Error, true))
            {
                logger.LogError("Request processed with errors: {Name} {@Response}", typeof(TResponse).Name, result);
            }
        }

        return result;
    }
}