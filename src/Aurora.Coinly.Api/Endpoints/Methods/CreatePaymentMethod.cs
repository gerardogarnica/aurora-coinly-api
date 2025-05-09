﻿using Aurora.Coinly.Application.Methods.Create;

namespace Aurora.Coinly.Api.Endpoints.Methods;

public sealed class CreatePaymentMethod : IBaseEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "methods",
            async ([FromBody] CreatePaymentMethodRequest request, ISender sender) =>
            {
                var command = new CreatePaymentMethodCommand(
                    request.Name,
                    request.IsDefault,
                    request.AllowRecurring,
                    request.AutoMarkAsPaid,
                    request.RelatedWalletId,
                    request.SuggestedPaymentDay,
                    request.StatementCutoffDay,
                    request.Notes);

                Result<Guid> result = await sender.Send(command);

                return result.Match(
                    () => Results.Created(string.Empty, result.Value),
                    ApiResponses.Problem);
            })
            .WithName("CreatePaymentMethod")
            .WithTags(EndpointTags.PaymentMethods)
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    internal sealed record CreatePaymentMethodRequest(
        string Name,
        bool IsDefault,
        bool AllowRecurring,
        bool AutoMarkAsPaid,
        Guid RelatedWalletId,
        int? SuggestedPaymentDay,
        int? StatementCutoffDay,
        string? Notes);
}