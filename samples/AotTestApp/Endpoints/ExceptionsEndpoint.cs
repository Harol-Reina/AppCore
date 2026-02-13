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
}
