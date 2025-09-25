using Aurora.Coinly.Application.Abstractions.Validations;
using FluentValidation.Results;

namespace Aurora.Coinly.Application.Abstractions.Behaviors;

internal static class ValidationBehavior
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        IEnumerable<IValidator<TCommand>> validators) : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
        where TResponse : Result
    {
        public async Task<Result<TResponse>> Handle(
            TCommand command,
            CancellationToken cancellationToken)
        {
            ValidationFailure[] failures = await ValidateAsync(command, validators);

            if (failures.Length == 0)
            {
                return await innerHandler.Handle(command, cancellationToken);
            }

            return Result.Fail<TResponse>(CreateValidationError(failures));
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        IEnumerable<IValidator<TCommand>> validators) : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            ValidationFailure[] failures = await ValidateAsync(command, validators);

            if (failures.Length == 0)
            {
                return await innerHandler.Handle(command, cancellationToken);
            }

            return Result.Fail(CreateValidationError(failures));
        }
    }

    private static async Task<ValidationFailure[]> ValidateAsync<TCommand>(
        TCommand command,
        IEnumerable<IValidator<TCommand>> validators)
    {
        if (!validators.Any())
        {
            return [];
        }

        var context = new ValidationContext<TCommand>(command);

        ValidationResult[] results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context)));

        ValidationFailure[] failures = [.. results
            .Where(x => !x.IsValid)
            .SelectMany(x => x.Errors)
            .Distinct()];

        return failures;
    }

    private static ValidationError CreateValidationError(ValidationFailure[] validationFailures) =>
        new([.. validationFailures.Select(f => new BaseError(f.ErrorCode, f.ErrorMessage))]);
}