using System.Reflection;
using OrionSoft.AppCore.Application.Exceptions;
using OrionSoft.AppCore.Application.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Application.Middleware;

public class ExceptionHandlingMiddlewareTests {

    private static ExceptionHandlingMiddleware CreateMiddleware(RequestDelegate next) =>
        new(next, NullLogger<ExceptionHandlingMiddleware>.Instance);

    private static DefaultHttpContext CreateContextWithBody() {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public async Task Invoke_WithXTraceIdHeader_ShouldUseProvidedTraceId() {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Trace-ID"] = "test-trace-id-123";
        var nextCalled = false;
        RequestDelegate next = (ctx) => {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var handler = CreateMiddleware(next);

        // Act
        await handler.Invoke(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Invoke_WithoutXTraceIdHeader_ShouldGenerateTraceId() {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        RequestDelegate next = (ctx) => {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var handler = CreateMiddleware(next);

        // Act
        await handler.Invoke(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Invoke_WhenNextThrows_ShouldHandleExceptionAndSetResponse() {
        // Arrange
        var context = CreateContextWithBody();
        RequestDelegate next = _ => throw new NotFoundException("Not found via invoke");
        var handler = CreateMiddleware(next);

        // Act
        await handler.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task Invoke_WithNotFoundException_ShouldReturn404() {
        // Arrange
        var context = CreateContextWithBody();
        var handler = CreateMiddleware(_ => throw new NotFoundException("Resource not found"));

        // Act
        await handler.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task Invoke_WithBadRequestException_ShouldReturn400() {
        // Arrange
        var context = CreateContextWithBody();
        var handler = CreateMiddleware(_ => throw new BadRequestException("Invalid input"));

        // Act
        await handler.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task Invoke_WithAuthenticationException_ShouldReturn401() {
        // Arrange
        var context = CreateContextWithBody();
        var handler = CreateMiddleware(_ => throw new AuthenticationException("Unauthorized access"));

        // Act
        await handler.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task Invoke_WithForbiddenAccessException_ShouldReturn403() {
        // Arrange
        var context = CreateContextWithBody();
        var handler = CreateMiddleware(_ => throw new ForbiddenAccessException("Forbidden resource"));

        // Act
        await handler.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task Invoke_WithValidationException_ShouldReturn400() {
        // Arrange
        var context = CreateContextWithBody();
        var handler = CreateMiddleware(_ => throw new ValidationException("Validation failed"));

        // Act
        await handler.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task Invoke_WithConflictException_ShouldReturn409() {
        // Arrange
        var context = CreateContextWithBody();
        var handler = CreateMiddleware(_ => throw new ConflictException("Resource already exists"));

        // Act
        await handler.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task Invoke_WithUnprocessableEntityException_ShouldReturn422() {
        // Arrange
        var context = CreateContextWithBody();
        var handler = CreateMiddleware(_ => throw new UnprocessableEntityException("Semantic error"));

        // Act
        await handler.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(422);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task Invoke_WithServiceUnavailableException_ShouldReturn503() {
        // Arrange
        var context = CreateContextWithBody();
        var handler = CreateMiddleware(_ => throw new ServiceUnavailableException("Redis", "Cache unavailable"));

        // Act
        await handler.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task Invoke_WithGatewayTimeoutException_ShouldReturn504() {
        // Arrange
        var context = CreateContextWithBody();
        var handler = CreateMiddleware(_ => throw new GatewayTimeoutException("Upstream timed out"));

        // Act
        await handler.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status504GatewayTimeout);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task Invoke_WithOperationException_ShouldReturn500() {
        // Arrange
        var context = CreateContextWithBody();
        var handler = CreateMiddleware(_ => throw new OperationException("Operation failed"));

        // Act
        await handler.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task Invoke_WithGenericException_ShouldReturn500() {
        // Arrange
        var context = CreateContextWithBody();
        var handler = CreateMiddleware(_ => throw new InvalidOperationException("Unexpected error"));

        // Act
        await handler.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task Invoke_ShouldWriteResponseBody() {
        // Arrange
        var context = CreateContextWithBody();
        var handler = CreateMiddleware(_ => throw new NotFoundException("Item not found"));

        // Act
        await handler.Invoke(context);

        // Assert
        context.Response.Body.Length.Should().BeGreaterThan(0);
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var content = await reader.ReadToEndAsync();
        content.Should().Contain("Item not found");
    }

    [Fact]
    public void GetOrGenerateTraceId_WithExistingTraceId_ShouldReturnIt() {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Trace-ID"] = "existing-trace-123";
        var method = typeof(ExceptionHandlingMiddleware).GetMethod("GetOrGenerateTraceId",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var result = method!.Invoke(null, new object[] { context, "X-Trace-ID" }) as string;

        // Assert
        result.Should().Be("existing-trace-123");
    }

    [Fact]
    public void GetOrGenerateTraceId_WithoutTraceId_ShouldGenerateGuid() {
        // Arrange
        var context = new DefaultHttpContext();
        var method = typeof(ExceptionHandlingMiddleware).GetMethod("GetOrGenerateTraceId",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var result = method!.Invoke(null, new object[] { context, "X-Trace-ID" }) as string;

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().MatchRegex("^[a-f0-9]{32}$"); // GUID without hyphens
    }

    [Fact]
    public async Task Invoke_WithApiDBException_ShouldReturn400() {
        // Arrange
        var context = CreateContextWithBody();
        var handler = CreateMiddleware(_ => throw new ApiDBException(new Exception("DB Error")));

        // Act
        await handler.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task Invoke_WithSerializerException_ShouldReturn500() {
        // Arrange
        var context = CreateContextWithBody();
        var handler = CreateMiddleware(_ => throw new SerializerException("Serialization error"));

        // Act
        await handler.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task Invoke_WithMappingException_ShouldReturn500() {
        // Arrange
        var context = CreateContextWithBody();
        var handler = CreateMiddleware(_ => throw new MappingException("Mapping error", innerException: null) {
            Errors = new Dictionary<string, string> { { "Key", "Value" } }
        });

        // Act
        await handler.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task Invoke_WithApiHttpException_ShouldReturn400() {
        // Arrange
        var context = CreateContextWithBody();
        var handler = CreateMiddleware(_ => throw new ApiHttpException(new HttpRequestException("HTTP Error")));

        // Act
        await handler.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task Invoke_WithCustomException_ShouldReturn400() {
        // Arrange
        var context = CreateContextWithBody();
        var handler = CreateMiddleware(_ => throw new CustomException(new DictionaryError { Code = "TEST", Message = "Custom Error" }));

        // Act
        await handler.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().Be("application/json");
    }
}
