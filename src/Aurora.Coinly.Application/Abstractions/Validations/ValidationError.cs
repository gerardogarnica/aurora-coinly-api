namespace Aurora.Coinly.Application.Abstractions.Validations;

internal sealed record ValidationError : BaseError
{
    internal BaseError[] Errors { get; }

    internal ValidationError(BaseError[] errors)
        : base("Validation", "One or more validation errors occurred")
    {
        Errors = errors;
    }

    internal static ValidationError FromResults(IEnumerable<Result> results) =>
        new([.. results.Where(r => !r.IsSuccessful).Select(r => r.Error)]);
}