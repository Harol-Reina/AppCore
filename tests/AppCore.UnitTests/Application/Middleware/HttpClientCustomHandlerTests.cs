using AppCore.Application.Exceptions;
using AppCore.Application.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using Xunit;

namespace AppCore.UnitTests.Application.Middleware;

public class HttpClientCustomHandlerTests
{
    [Fact]
    public async Task Invoke_WithXTraceIdHeader_ShouldUseProvidedTraceId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Trace-ID"] = "test-trace-id-123";
        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var handler = new HttpClientCustomHandler(next);

        // Act
        await handler.Invoke(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Invoke_WithoutXTraceIdHeader_ShouldGenerateTraceId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var handler = new HttpClientCustomHandler(next);

        // Act
        await handler.Invoke(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task HandleExceptionAsync_WithNotFoundException_ShouldReturn404()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new NotFoundException("Resource not found");

        // Act
        await HttpClientCustomHandler.HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task HandleExceptionAsync_WithBadRequestException_ShouldReturn400()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new BadRequestException("Invalid input");

        // Act
        await HttpClientCustomHandler.HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task HandleExceptionAsync_WithAuthenticationException_ShouldReturn401()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new AuthenticationException("Unauthorized access");

        // Act
        await HttpClientCustomHandler.HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task HandleExceptionAsync_WithForbiddenAccessException_ShouldReturn403()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new ForbiddenAccessException("Forbidden resource");

        // Act
        await HttpClientCustomHandler.HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task HandleExceptionAsync_WithValidationException_ShouldReturn400()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new ValidationException("Validation failed");

        // Act
        await HttpClientCustomHandler.HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task HandleExceptionAsync_WithGenericException_ShouldReturn500()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new InvalidOperationException("Unexpected error");

        // Act
        await HttpClientCustomHandler.HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task HandleExceptionAsync_ShouldWriteResponseBody()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        var exception = new NotFoundException("Item not found");

        // Act
        await HttpClientCustomHandler.HandleExceptionAsync(context, exception);
        
        // Assert
        responseBody.Length.Should().BeGreaterThan(0);
        responseBody.Position = 0;
        using var reader = new StreamReader(responseBody);
        var content = await reader.ReadToEndAsync();
        content.Should().Contain("Item not found");
    }

    [Fact]
    public void GetOrGenerateTraceId_WithExistingTraceId_ShouldReturnIt()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Trace-ID"] = "existing-trace-123";
        var method = typeof(HttpClientCustomHandler).GetMethod("GetOrGenerateTraceId",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var result = method!.Invoke(null, new object[] { context, "X-Trace-ID" }) as string;

        // Assert
        result.Should().Be("existing-trace-123");
    }

    [Fact]
    public void GetOrGenerateTraceId_WithoutTraceId_ShouldGenerateGuid()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var method = typeof(HttpClientCustomHandler).GetMethod("GetOrGenerateTraceId",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var result = method!.Invoke(null, new object[] { context, "X-Trace-ID" }) as string;

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().MatchRegex("^[a-f0-9]{32}$"); // GUID without hyphens
    }
}
