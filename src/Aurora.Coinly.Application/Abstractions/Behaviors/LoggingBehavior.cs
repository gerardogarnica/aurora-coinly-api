﻿using Microsoft.Extensions.Logging;

namespace Aurora.Coinly.Application.Abstractions.Behaviors;

internal static class LoggingBehavior
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        ILogger<CommandHandler<TCommand, TResponse>> logger) : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
        where TResponse : Result
    {
        public async Task<Result<TResponse>> Handle(
            TCommand command,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing request: {Name} {@Request}", typeof(TCommand).Name, command);

            Result<TResponse> result = await innerHandler.Handle(command, cancellationToken);

            if (result.IsSuccessful)
            {
                logger.LogInformation("Request processed successfully: {Name} {@Response}", typeof(TResponse).Name, result);
            }
            else
            {
                logger.LogError("Request processed with errors: {Name} {@Response}", typeof(TResponse).Name, result);
            }

            return result;
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        ILogger<CommandBaseHandler<TCommand>> logger) : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> Handle(
            TCommand command,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing request: {Name} {@Request}", typeof(TCommand).Name, command);

            Result result = await innerHandler.Handle(command, cancellationToken);

            if (result.IsSuccessful)
            {
                logger.LogInformation("Request processed successfully: {Name} {@Response}", typeof(TCommand).Name, result);
            }
            else
            {
                logger.LogError("Request processed with errors: {Name} {@Response}", typeof(TCommand).Name, result);
            }

            return result;
        }
    }

    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> innerHandler,
        ILogger<QueryHandler<TQuery, TResponse>> logger) : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
        where TResponse : Result
    {
        public async Task<Result<TResponse>> Handle(
            TQuery query,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing request: {Name} {@Request}", typeof(TQuery).Name, query);

            Result<TResponse> result = await innerHandler.Handle(query, cancellationToken);

            if (result.IsSuccessful)
            {
                logger.LogInformation("Request processed successfully: {Name} {@Response}", typeof(TResponse).Name, result);
            }
            else
            {
                logger.LogError("Request processed with errors: {Name} {@Response}", typeof(TResponse).Name, result);
            }

            return result;
        }
    }
}