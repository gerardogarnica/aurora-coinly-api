namespace Aurora.Coinly.Api.Responses;

internal static class ApiResponses
{
    internal static IResult Problem(Result result)
    {
        if (result.IsSuccessful)
        {
            throw new InvalidOperationException("The result is successful. Can't convert succesful result to a problem.");
        }

        return Results.Problem(
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title: $"The result of the request is a {result.Error.Code} failure",
            statusCode: StatusCodes.Status400BadRequest,
            detail: result.Error.Message,
            extensions: GetErrors(result)
        );
    }

    private static Dictionary<string, object?> GetErrors(Result result)
    {
        if (result.Error is not Application.Abstractions.Validations.ValidationError validationError)
        {
            return [];
        }

        return new Dictionary<string, object?>
        {
            { "errors", validationError.Errors }
        };
    }
}