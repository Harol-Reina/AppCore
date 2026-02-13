using OrionSoft.AppCore.Application.Exceptions;
using OrionSoft.AppCore.Application.Interfaces;
using OrionSoft.AppCore.Application.Wrappers;

namespace AotTestApp.Endpoints;

public class ExceptionsEndpoint : IEndpointGroupBase {
    public void MapEndpoints(RouteGroupBuilder group, string groupName) {
        group.MapGet("/success", GetSuccess)
            .Produces<Response<string>>();

        group.MapGet("/not-found", GetNotFound);

        group.MapGet("/bad-request", GetBadRequest);

        group.MapGet("/validation", GetValidation);

        group.MapGet("/validation-multiple", GetValidationMultiple);

        group.MapGet("/unauthorized", GetUnauthorized);

        group.MapGet("/forbidden", GetForbidden);

        group.MapGet("/custom-error", GetCustomError);

        group.MapGet("/unhandled", GetUnhandled);

        group.MapGet("/conflict", GetConflict);

        group.MapGet("/unprocessable", GetUnprocessable);

        group.MapGet("/service-unavailable", GetServiceUnavailable);

        group.MapGet("/gateway-timeout", GetGatewayTimeout);
    }

    private static IResult GetSuccess() {
        return Results.Ok(Response<string>.Success("OK", "Exception endpoints are working"));
    }

    private static IResult GetNotFound() {
        throw new NotFoundException("Resource not found for testing");
    }

    private static IResult GetBadRequest() {
        throw new BadRequestException("Bad request for testing");
    }

    private static IResult GetValidation() {
        throw new ValidationException("Field", "Field is required");
    }

    private static IResult GetValidationMultiple() {
        throw new ValidationException([
            new FluentValidation.Results.ValidationFailure("Name", "Name is required"),
            new FluentValidation.Results.ValidationFailure("Name", "Name must be at least 3 characters"),
            new FluentValidation.Results.ValidationFailure("Email", "Email is invalid")
        ]);
    }

    private static IResult GetUnauthorized() {
        throw new AuthenticationException("Authentication required for testing");
    }

    private static IResult GetForbidden() {
        throw new ForbiddenAccessException("Access forbidden for testing");
    }

    private static IResult GetCustomError() {
        throw new CustomException(new DictionaryError("CUSTOM-001", "Custom error for testing"));
    }

    private static IResult GetUnhandled() {
        throw new OperationException("Unhandled operation error for testing");
    }

    private static IResult GetConflict() {
        throw new ConflictException("User", "john@test.com");
    }

    private static IResult GetUnprocessable() {
        throw new UnprocessableEntityException("The entity cannot be processed due to semantic errors");
    }

    private static IResult GetServiceUnavailable() {
        throw new ServiceUnavailableException("PaymentGateway", "Payment service is currently unavailable");
    }

    private static IResult GetGatewayTimeout() {
        throw new GatewayTimeoutException("Upstream service did not respond in time");
    }
}
